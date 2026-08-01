using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

public class OrderController : Controller
{
    private readonly SoleStrideDbContext _context;

    public OrderController(SoleStrideDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var role = HttpContext.Session.GetString("Role");
        List<Order> orders;
        if (role == "Admin" || role == "Staff")
        {
            orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
        }
        else
        {
            orders = await _context.Orders.Where(o => o.Username == username).OrderByDescending(o => o.OrderDate).ToListAsync();
        }
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var username = HttpContext.Session.GetString("Username");
        if (username == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var role = HttpContext.Session.GetString("Role");
        var order = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(d => d.Product).FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null) return NotFound();

        if (role != "Admin" && role != "Staff" && order.Username != username)
            return RedirectToAction(nameof(Index));

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, string status)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff") return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}
