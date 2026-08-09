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
    public async Task<IActionResult> PlaceOrder(string shippingAddress, string? receiverName, string phone, string? customerNote)
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

        foreach (var item in cart)
        {
            var availableStockCount = await _context.ShoeStocks
                .Where(s => s.ProductId == item.ProductId && s.Status == ShoeStock.InventoryStatus.Available)
                .CountAsync();

            if (availableStockCount < item.Quantity)
            {
                TempData["CheckoutError"] = $"Insufficient stock for {item.ShoesName}. Available: {availableStockCount}, Requested: {item.Quantity}.";
                return RedirectToAction(nameof(Index));
            }
        }

        Order order = null;
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            order = new Order
            {
                Username = username,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.Subtotal),
                Status = "Pending",
                ShippingAddress = shippingAddress,
                ReceiverName = string.IsNullOrWhiteSpace(receiverName) ? username : receiverName.Trim(),
                Phone = phone,
                CustomerNote = customerNote
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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

                var stocksToSell = await _context.ShoeStocks
                    .Where(s => s.ProductId == item.ProductId && s.Status == ShoeStock.InventoryStatus.Available)
                    .Take(item.Quantity)
                    .ToListAsync();

                foreach (var stock in stocksToSell)
                {
                    stock.Status = ShoeStock.InventoryStatus.Sold;
                    stock.PurchaseDate = DateTime.Now;

                    _context.OrderStocks.Add(new OrderStock
                    {
                        OrderDetailId = orderDetail.OrderDetailId,
                        StockId = stock.StockId
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        SaveCart(new List<CartItem>());
        TempData["OrderSuccess"] = "Order placed successfully!";
        return RedirectToAction("Details", "Order", new { id = order.OrderId });
    }
}
