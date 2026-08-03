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
        return data == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(data) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }

    [HttpGet]
    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var cart = GetCart();
        if (!cart.Any()) return RedirectToAction("Index", "Cart");

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string shippingAddress, string phone)
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var cart = GetCart();
        if (!cart.Any()) return RedirectToAction("Index", "Cart");

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            TempData["CheckoutError"] = "Shipping address is required.";
            return RedirectToAction(nameof(Index));
        }

        var order = new Order
        {
            Username = username,
            OrderDate = DateTime.Now,
            TotalAmount = cart.Sum(i => i.Subtotal),
            Status = "Pending",
            ShippingAddress = shippingAddress,
            Phone = phone
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in cart)
        {
            _context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.FinalPrice
            });
        }
        await _context.SaveChangesAsync();

        SaveCart(new List<CartItem>());
        TempData["OrderSuccess"] = "Order placed successfully!";
        return RedirectToAction("Details", "Order", new { id = order.OrderId });
    }
}
