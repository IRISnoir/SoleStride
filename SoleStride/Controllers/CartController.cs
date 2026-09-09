using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SoleStride.Models;

public class CartController : Controller
{
    private readonly SoleStrideDbContext _context;

    public CartController(SoleStrideDbContext context)
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

    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
    {
        var shoes = await _context.Shoes.FindAsync(productId);
        if (shoes == null) return NotFound();

        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = shoes.ProductId,
                ShoesName = shoes.ShoesName,
                ImageUrl = shoes.ImageUrl,
                Price = shoes.Price,
                SalePercentage = shoes.SalePercentage,
                Quantity = quantity
            });
        }
        SaveCart(cart);
        TempData["CartMessage"] = $"Added {shoes.ShoesName} to cart.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult UpdateQuantity(Guid productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
        }
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(Guid productId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.ProductId == productId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult CartCount()
    {
        var cart = GetCart();
        return Json(cart.Sum(c => c.Quantity));
    }
}
