using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

public class StockController : Controller
{
    private readonly SoleStrideDbContext _context;

    public StockController(SoleStrideDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Index", "Home");

        var stock = await _context.ShoeStocks.Include(s => s.Shoes).OrderByDescending(s => s.EntryDate).ToListAsync();
        return View(stock);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Index", "Home");

        ViewBag.Products = await _context.Shoes.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid productId, int quantity)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Index", "Home");

        for (int i = 0; i < quantity; i++)
        {
            _context.ShoeStocks.Add(new ShoeStock
            {
                ProductId = productId,
                Status = ShoeStock.InventoryStatus.Available,
                EntryDate = DateTime.Now
            });
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int stockId)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Index", "Home");

        var stock = await _context.ShoeStocks.FindAsync(stockId);
        if (stock != null)
        {
            _context.ShoeStocks.Remove(stock);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkSold(int stockId)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Index", "Home");

        var stock = await _context.ShoeStocks.FindAsync(stockId);
        if (stock != null)
        {
            stock.Status = ShoeStock.InventoryStatus.Sold;
            stock.PurchaseDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
