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

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("Role") == "Staff";
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        List<Order> orders;
        if (IsAdmin() || IsStaff())
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

        var order = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(d => d.Product).FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null) return NotFound();

        if (!IsAdmin() && !IsStaff() && order.Username != username)
            return RedirectToAction(nameof(Index));

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, string status)
    {
        if (!IsAdmin() && !IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;

            if (order.Status == "Cancelled")
            {
                var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

                var orderDetailIds = orderDetails.Select(od => od.OrderDetailId).ToList();

                var orderStocks = await _context.OrderStocks
                    .Where(os => orderDetailIds.Contains(os.OrderDetailId))
                    .ToListAsync();

                foreach (var orderStock in orderStocks)
                {
                    var shoeStock = await _context.ShoeStocks.FindAsync(orderStock.StockId);
                    if (shoeStock != null)
                    {
                        shoeStock.Status = ShoeStock.InventoryStatus.Available;
                        shoeStock.PurchaseDate = null;
                    }
                }
            }

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

            var orderDetails = await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();

            var orderDetailIds = orderDetails.Select(od => od.OrderDetailId).ToList();

            var orderStocks = await _context.OrderStocks
                .Where(os => orderDetailIds.Contains(os.OrderDetailId))
                .ToListAsync();

            foreach (var orderStock in orderStocks)
            {
                var shoeStock = await _context.ShoeStocks.FindAsync(orderStock.StockId);
                if (shoeStock != null)
                {
                    shoeStock.Status = ShoeStock.InventoryStatus.Available;
                    shoeStock.PurchaseDate = null;
                }
            }

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}
