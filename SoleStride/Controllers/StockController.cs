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

    public bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    public bool IsStaff()
    {
        return HttpContext.Session.GetString("Role") == "Staff";
    }

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var stock = await _context.ShoeStocks.Include(s => s.Shoes).OrderByDescending(s => s.EntryDate).ToListAsync();
        return View(stock);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        ViewBag.Products = await _context.Shoes.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid productId, int quantity)
    {
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

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
        if (!IsAdmin()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

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
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var stock = await _context.ShoeStocks.FindAsync(stockId);
        if (stock != null)
        {
            if (stock.Status == ShoeStock.InventoryStatus.Available)
            {
                stock.Status = ShoeStock.InventoryStatus.Sold;
                stock.PurchaseDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            else
            {
                stock.Status = ShoeStock.InventoryStatus.Available;
                stock.PurchaseDate = null;
                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MarkDamaged(int stockId)
    {
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var stock = await _context.ShoeStocks.FindAsync(stockId);
        if (stock != null)
        {
            if (stock.Status == ShoeStock.InventoryStatus.Available)
            {
                stock.Status = ShoeStock.InventoryStatus.Damaged;
                await _context.SaveChangesAsync();
            }
            else if (stock.Status == ShoeStock.InventoryStatus.Damaged)
            {
                stock.Status = ShoeStock.InventoryStatus.Available;
                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction(nameof(Index));
    }
}
