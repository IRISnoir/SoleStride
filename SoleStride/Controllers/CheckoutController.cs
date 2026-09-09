using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

public class CheckoutController : Controller
{
    private readonly SoleStrideDbContext _context;

    public CheckoutController(SoleStrideDbContext context)
    {
        _context = context;
    }

    private List<CartItem> GetCart()
    {
        var data = HttpContext.Session.GetString("Cart");

        return data == null
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(data)
                ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString(
            "Cart",
            JsonSerializer.Serialize(cart));
    }

    // =========================
    // CHECKOUT
    // =========================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Request.Path + Request.QueryString
                });
        }

        var cart = GetCart();

        if (!cart.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        // Lấy sổ địa chỉ của user đang đăng nhập
        var addresses = await _context.Addresses
            .Where(a => a.Username == username)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.AddressId)
            .ToListAsync();

        // Gửi danh sách địa chỉ sang View
        ViewBag.Addresses = addresses;

        return View(cart);
    }

    // =========================
    // CREATE ADDRESS
    // =========================

    [HttpGet]
    public IActionResult CreateAddress()
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.DefaultName = username;

        return View("CreateAddress");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(Address address)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("ERROR: " + error.ErrorMessage);
            }

            return View("CreateAddress", address);
        }

        address.Username = username;

        // Nếu chưa có địa chỉ nào thì tự động làm mặc định
        var hasAddress = await _context.Addresses
            .AnyAsync(a => a.Username == username);

        if (!hasAddress)
        {
            address.IsDefault = true;
        }

        // Nếu địa chỉ mới là mặc định
        if (address .IsDefault)
        {
            var oldAddresses = await _context.Addresses
                .Where(a => a.Username == username)
                .ToListAsync();

            foreach (var item in oldAddresses)
            {
                item.IsDefault = false;
            }
        }

        _context.Addresses.Add(address);

        await _context.SaveChangesAsync();

        TempData["AddressSuccess"] = "Address added successfully.";

        return RedirectToAction("Index", "Checkout");
    }

    // =========================
    // EDIT ADDRESS
    // =========================

    [HttpGet]
    public async Task<IActionResult> EditAddress(int id)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a =>
                a.AddressId == id &&
                a.Username == username);

        if (address == null)
        {
            return NotFound();
        }

        return View(address);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(
        int id,
        string recipientName,
        string phone,
        string addressLine,
        bool isDefault)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a =>
                a.AddressId == id &&
                a.Username == username);

        if (address == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(recipientName) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(addressLine))
        {
            TempData["AddressError"] =
                "Please fill in all address information.";

            return RedirectToAction(
                nameof(EditAddress),
                new { id });
        }

        address.RecipientName = recipientName;
        address.Phone = phone;
        address.AddressLine = addressLine;

        if (isDefault)
        {
            var otherAddresses = await _context.Addresses
                .Where(a =>
                    a.Username == username &&
                    a.AddressId != id)
                .ToListAsync();

            foreach (var item in otherAddresses)
            {
                item.IsDefault = false;
            }

            address.IsDefault = true;
        }

        await _context.SaveChangesAsync();

        TempData["AddressSuccess"] =
            "Address updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // DELETE ADDRESS
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a =>
                a.AddressId == id &&
                a.Username == username);

        if (address == null)
        {
            return NotFound();
        }

        bool wasDefault = address.IsDefault;

        _context.Addresses.Remove(address);

        await _context.SaveChangesAsync();

        // Nếu xoá địa chỉ mặc định,
        // chọn địa chỉ đầu tiên làm mặc định
        if (wasDefault)
        {
            var newDefault = await _context.Addresses
                .Where(a => a.Username == username)
                .OrderByDescending(a => a.AddressId)
                .FirstOrDefaultAsync();

            if (newDefault != null)
            {
                newDefault.IsDefault = true;

                await _context.SaveChangesAsync();
            }
        }

        TempData["AddressSuccess"] =
            "Address deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // SET DEFAULT ADDRESS
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(
    int addressId,
    string? customerNote)
    {
        var username = HttpContext.Session.GetString("Username");

        if (username == null)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Request.Path + Request.QueryString
                });
        }

        var cart = GetCart();

        if (!cart.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        // =========================
        // 1. KIỂM TRA ĐỊA CHỈ
        // =========================

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a =>
                a.AddressId == addressId &&
                a.Username == username);

        if (address == null)
        {
            TempData["CheckoutError"] =
                "Please select a valid shipping address.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // 2. KIỂM TRA TỒN KHO
        // =========================

        foreach (var item in cart)
        {
            var availableStockCount = await _context.ShoeStocks
                .CountAsync(s =>
                    s.ProductId == item.ProductId &&
                    s.Status == ShoeStock.InventoryStatus.Available);

            if (availableStockCount < item.Quantity)
            {
                TempData["CheckoutError"] =
                    $"Insufficient stock for {item.ShoesName}. " +
                    $"Available: {availableStockCount}, " +
                    $"Requested: {item.Quantity}.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================
        // 3. TẠO ORDER
        // =========================

        var order = new Order
        {
            Username = username,
            OrderDate = DateTime.Now,
            TotalAmount = cart.Sum(i => i.Subtotal),
            Status = "Pending",

            ShippingAddress = address.AddressLine,
            Phone = address.Phone,
            ReceiverName = address.RecipientName,

            CustomerNote = customerNote
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();


        // =========================
        // 4. TẠO ORDER DETAILS
        // =========================

        foreach (var item in cart)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.FinalPrice
            };

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            var stocks = await _context.ShoeStocks
                .Where(s =>
                    s.ProductId == item.ProductId &&
                    s.Status == ShoeStock.InventoryStatus.Available)
                .Take(item.Quantity)
                .ToListAsync();

            foreach (var stock in stocks)
            {
                stock.Status = ShoeStock.InventoryStatus.Sold;

                _context.OrderStocks.Add(new OrderStock
                {
                    OrderDetailId = orderDetail.OrderDetailId,
                    StockId = stock.StockId
                });
            }
        }

        await _context.SaveChangesAsync();


        // =========================
        // 6. XÓA GIỎ HÀNG
        // =========================

        SaveCart(new List<CartItem>());


        TempData["OrderSuccess"] =
            "Order placed successfully!";


        // =========================
        // 7. ĐI ĐẾN ORDER DETAILS
        // =========================

        return RedirectToAction(
            "Details",
            "Order",
            new { id = order.OrderId });
    }
}