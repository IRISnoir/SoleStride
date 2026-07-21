using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;
using System.Security.Cryptography;

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
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Username,Password,Role,Phone,EmailAddress,Birthdate,UserGender")] User user)
        {
            if (ModelState.IsValid)
            {
                string passPlaintext = user.Password;
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passPlaintext));
                    user.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            string passPlaintext = password;
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passPlaintext));
                password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }

            if (user == null || user.Password != password)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.ToString());

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
