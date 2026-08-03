using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;
using System.Diagnostics;

namespace SoleStride.Controllers
{
    public class HomeController : Controller
    {
        private readonly SoleStrideDbContext _context;

        public HomeController(SoleStrideDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var shoes = await _context.Shoes
                .Include(s => s.Category)
                .OrderBy(s => s.ShoesName)
                .ToListAsync();

            // Create view models with stock data for each shoe
            var homeProducts = new List<HomeProductViewModel>();
            foreach (var shoe in shoes)
            {
                var quantityAvailable = await _context.ShoeStocks
                    .CountAsync(s => s.ProductId == shoe.ProductId && s.Status == ShoeStock.InventoryStatus.Available);

                var quantitySold = await _context.ShoeStocks
                    .CountAsync(s => s.ProductId == shoe.ProductId && s.Status == ShoeStock.InventoryStatus.Sold);

                homeProducts.Add(new HomeProductViewModel
                {
                    Shoes = shoe,
                    QuantityAvailable = quantityAvailable,
                    QuantitySold = quantitySold
                });
            }

            return View(homeProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var results = await _context.Shoes
                .Include(s => s.Category)
                .Where(s => s.ShoesName.Contains(query)
                    || (s.Description != null && s.Description.Contains(query))
                    || (s.Category != null && s.Category.CategoryName.Contains(query))
                    || s.ShoesColor.Contains(query)
                    || s.Material.Contains(query))
                .OrderBy(s => s.ShoesName)
                .ToListAsync();

            ViewBag.Query = query;
            return View(results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
