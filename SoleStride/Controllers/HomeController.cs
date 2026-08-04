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

        public async Task<IActionResult> Index(
            string? categoryId,
            string? gender,
            int? minSize,
            int? maxSize,
            string? color,
            decimal? minPrice,
            decimal? maxPrice,
            bool? onSale,
            string? sortBy)
        {
            var query = _context.Shoes
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoryId))
                query = query.Where(s => s.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(s => s.ShoesGender.ToString() == gender);

            if (minSize.HasValue)
                query = query.Where(s => s.ShoesSize >= minSize.Value);

            if (maxSize.HasValue)
                query = query.Where(s => s.ShoesSize <= maxSize.Value);

            if (!string.IsNullOrEmpty(color))
                query = query.Where(s => s.ShoesColor.Contains(color));

            if (minPrice.HasValue)
                query = query.Where(s => s.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(s => s.Price <= maxPrice.Value);

            if (onSale == true)
                query = query.Where(s => s.SalePercentage.HasValue && s.SalePercentage > 0);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(s => s.Price),
                "price_desc" => query.OrderByDescending(s => s.Price),
                "name_desc" => query.OrderByDescending(s => s.ShoesName),
                "newest" => query.OrderByDescending(s => s.ProductId),
                _ => query.OrderBy(s => s.ShoesName)
            };

            var shoes = await query.ToListAsync();
            var categories = await _context.Category.OrderBy(c => c.CategoryName).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedGender = gender;
            ViewBag.MinSize = minSize;
            ViewBag.MaxSize = maxSize;
            ViewBag.SelectedColor = color;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.OnSale = onSale;
            ViewBag.SortBy = sortBy;

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
