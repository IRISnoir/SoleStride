using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace SoleStride.Controllers
{
    public class AccountController : Controller
    {
        private readonly SoleStrideDbContext _context;

        public AccountController(SoleStrideDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Username,Password,FirstName,LastName,Phone,EmailAddress,Birthdate,UserGender")] User user, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(user);
                }

                user.Role = Models.User.UserRole.User;

                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user.Password));
                    user.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login), new { returnUrl = returnUrl });
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                returnUrl = Url.Action("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            if (user == null || user.Password != password)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.ToString());
            HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl ?? "");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string firstName, string lastName, string? phone, string? emailAddress, DateTime birthdate, string userGender, IFormFile? avatarFile)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            var age = DateTime.Now.Year - birthdate.Year;
            if (birthdate > DateTime.Now.AddYears(-age)) age--;
            if (emailAddress == null || !new EmailAddressAttribute().IsValid(emailAddress))
            {
                TempData["ProfileError"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(EditProfile));
            }
            if (age < 14)
            {
                TempData["ProfileError"] = "You must be at least 14 years old.";
                return RedirectToAction(nameof(EditProfile));
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Phone = phone;
            user.EmailAddress = emailAddress;
            user.Birthdate = birthdate;
            user.UserGender = Enum.Parse<User.Gender>(userGender);

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }
                user.AvatarUrl = "/avatars/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl ?? "");

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(currentPassword));
                currentPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            if (user.Password != currentPassword)
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction(nameof(EditProfile));
            }

            if (newPassword != confirmPassword)
            {
                TempData["PasswordError"] = "New passwords do not match.";
                return RedirectToAction(nameof(EditProfile));
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["PasswordError"] = "Password must be at least 6 characters.";
                return RedirectToAction(nameof(EditProfile));
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
                user.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            await _context.SaveChangesAsync();
            TempData["PasswordSuccess"] = "Password changed successfully.";
            return RedirectToAction(nameof(EditProfile));
        }

        // GET: Forgot password - step 1, enter username
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot password - step 1, verify username and generate random code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Please enter your username.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "Username not found. Please check and try again.";
                return View();
            }

            var code = GenerateResetCode();
            HttpContext.Session.SetString("ResetUsername", user.Username);
            HttpContext.Session.SetString("ResetCode", code);

            ViewBag.Username = user.Username;
            ViewBag.EmailAddress = user.EmailAddress;
            ViewBag.ResetCode = code;
            return View("VerifyCode");
        }

        // GET/POST: Forgot password - step 2, enter the random code
        [HttpGet]
        public IActionResult VerifyCode()
        {
            var username = HttpContext.Session.GetString("ResetUsername");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyCode(string code)
        {
            var username = HttpContext.Session.GetString("ResetUsername");
            var storedCode = HttpContext.Session.GetString("ResetCode");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(storedCode))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Username = username;
            ViewBag.ResetCode = storedCode;

            if (string.IsNullOrWhiteSpace(code) || !string.Equals(code.Trim(), storedCode, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Incorrect code. Please try again.";
                return View();
            }

            return View("ResetPassword");
        }

        // GET/POST: Forgot password - step 3, set a new password
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var username = HttpContext.Session.GetString("ResetUsername");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }
            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var username = HttpContext.Session.GetString("ResetUsername");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New passwords do not match.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                user.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("ResetUsername");
            HttpContext.Session.Remove("ResetCode");

            TempData["ResetSuccess"] = "Password reset successfully. Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        private static string GenerateResetCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
            var random = new Random();
            var code = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                code.Append(chars[random.Next(chars.Length)]);
            }
            return code.ToString();
        }
    }
}
