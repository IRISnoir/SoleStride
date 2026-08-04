using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

namespace SoleStride.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SoleStrideDbContext _context;

        public DashboardController(SoleStrideDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();

            var shoes = await _context.Shoes.Include(s => s.Category).ToListAsync();
            var stock = await _context.ShoeStocks.ToListAsync();

            var successfulOrders = orders.Where(o => o.Status != "Cancelled").ToList();
            var cancelledOrderIds = orders.Where(o => o.Status == "Cancelled").Select(o => o.OrderId).ToHashSet();
            var saleDetails = orders
                .SelectMany(o => o.OrderDetails)
                .Where(d => !cancelledOrderIds.Contains(d.OrderId))
                .ToList();

            var model = new DashboardViewModel
            {
                TotalProducts = shoes.Count,
                TotalOrders = orders.Count,
                TotalRevenue = successfulOrders.Sum(o => o.TotalAmount),
                TotalUsers = await _context.Users.CountAsync(),

                PendingOrders = orders.Count(o => o.Status == "Pending"),
                ProcessingOrders = orders.Count(o => o.Status == "Processing"),
                ShippedOrders = orders.Count(o => o.Status == "Shipped"),
                DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),

                StockAvailable = stock.Count(s => s.Status == ShoeStock.InventoryStatus.Available),
                StockSold = stock.Count(s => s.Status == ShoeStock.InventoryStatus.Sold),
                StockDamaged = stock.Count(s => s.Status == ShoeStock.InventoryStatus.Damaged),

                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(10).ToList(),

                BestSellers = shoes
                    .Select(s => new BestSellerItem
                    {
                        ProductId = s.ProductId,
                        ShoesName = s.ShoesName,
                        CategoryName = s.Category?.CategoryName ?? "-",
                        Price = s.Price,
                        TotalSold = saleDetails.Where(d => d.ProductId == s.ProductId).Sum(d => d.Quantity)
                    })
                    .Where(b => b.TotalSold > 0)
                    .OrderByDescending(b => b.TotalSold)
                    .Take(10)
                    .ToList(),

                SalesByCategory = shoes
                    .GroupBy(s => s.Category?.CategoryName ?? "Unknown")
                    .Select(g => new CategorySaleItem
                    {
                        CategoryName = g.Key,
                        Revenue = g.Sum(s => saleDetails.Where(d => d.ProductId == s.ProductId).Sum(d => d.Quantity * d.Price))
                    })
                    .Where(c => c.Revenue > 0)
                    .OrderByDescending(c => c.Revenue)
                    .ToList(),

                LowStockProducts = shoes
                    .Where(s => stock.Count(x => x.ProductId == s.ProductId && x.Status == ShoeStock.InventoryStatus.Available) <= 3)
                    .OrderBy(s => stock.Count(x => x.ProductId == s.ProductId && x.Status == ShoeStock.InventoryStatus.Available))
                    .ToList(),
                AvailableCounts = shoes.ToDictionary(
                    s => s.ProductId,
                    s => stock.Count(x => x.ProductId == s.ProductId && x.Status == ShoeStock.InventoryStatus.Available))
            };

            return View(model);
        }
    }
}
