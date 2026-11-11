
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using SoleStride.Models;

public class ShoesController : Controller
{
    private readonly SoleStrideDbContext _context;

    public ShoesController(SoleStrideDbContext context)
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

    // GET: SHOESS
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
        var query = _context.Shoes.Include(s => s.Category).AsQueryable();

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

        if (IsAdmin() || IsStaff())
        {
            var productsWithNewReviews = await _context.ProductReviews
                .Where(r => string.IsNullOrEmpty(r.ShopReply) || r.CreatedAt >= DateTime.Now.AddDays(-2))
                .Select(r => r.ProductId)
                .Distinct()
                .ToListAsync();

            ViewBag.ProductsWithNewReviews = new HashSet<Guid>(productsWithNewReviews);
        }

        return View(shoes);
    }

    // GET: SHOESS/Details/5
    public async Task<IActionResult> Details(System.Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shoes = await _context.Shoes
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.ProductId == id);
        if (shoes == null)
        {
            return NotFound();
        }

        // Fetch Product Reviews
        var userRole = HttpContext.Session.GetString("Role");
        var isStaffOrAdmin = userRole == "Admin" || userRole == "Staff";

        // Admin/Staff sees all reviews, normal users see visible only
        var reviewsQuery = _context.ProductReviews
            .Where(r => r.ProductId == id);

        if (!isStaffOrAdmin)
        {
            reviewsQuery = reviewsQuery.Where(r => r.IsVisible);
        }

        var reviews = await reviewsQuery
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var totalReviews = reviews.Count;
        var avgRating = totalReviews > 0 ? reviews.Average(r => r.Rating) : 0.0;

        // Rating distribution (5 down to 1 star)
        var ratingDist = new Dictionary<int, int>
        {
            { 5, reviews.Count(r => r.Rating == 5) },
            { 4, reviews.Count(r => r.Rating == 4) },
            { 3, reviews.Count(r => r.Rating == 3) },
            { 2, reviews.Count(r => r.Rating == 2) },
            { 1, reviews.Count(r => r.Rating == 1) },
        };

        // Fit breakdown
        var runsSmallCount = reviews.Count(r => r.FitFeedback == ShoeFit.RunsSmall);
        var trueToSizeCount = reviews.Count(r => r.FitFeedback == ShoeFit.TrueToSize);
        var runsLargeCount = reviews.Count(r => r.FitFeedback == ShoeFit.RunsLarge);

        var runsSmallPct = totalReviews > 0 ? (int)Math.Round((double)runsSmallCount / totalReviews * 100) : 0;
        var trueToSizePct = totalReviews > 0 ? (int)Math.Round((double)trueToSizeCount / totalReviews * 100) : 0;
        var runsLargePct = totalReviews > 0 ? (int)Math.Round((double)runsLargeCount / totalReviews * 100) : 0;

        // User specific state
        var currentUsername = HttpContext.Session.GetString("Username");
        ProductReview? currentUserReview = null;
        var isVerifiedBuyer = false;

        if (!string.IsNullOrEmpty(currentUsername))
        {
            currentUserReview = reviews.FirstOrDefault(r => r.Username == currentUsername);

            isVerifiedBuyer = await _context.Orders
                .Where(o => o.Username == currentUsername && o.Status == "Delivered")
                .AnyAsync(o => o.OrderDetails.Any(od => od.ProductId == id));
        }

        ViewBag.Reviews = reviews;
        ViewBag.TotalReviews = totalReviews;
        ViewBag.AverageRating = Math.Round(avgRating, 1);
        ViewBag.RatingDistribution = ratingDist;
        ViewBag.RunsSmallPct = runsSmallPct;
        ViewBag.TrueToSizePct = totalReviews > 0 && runsSmallPct == 0 && runsLargePct == 0 ? 100 : trueToSizePct;
        ViewBag.RunsLargePct = runsLargePct;
        ViewBag.CurrentUserReview = currentUserReview;
        ViewBag.IsVerifiedBuyer = isVerifiedBuyer;

        // Check for new reviews for seller (Admin/Staff)
        var newReviewsForSeller = reviews
            .Where(r => string.IsNullOrEmpty(r.ShopReply) || r.CreatedAt >= DateTime.Now.AddDays(-2))
            .ToList();

        ViewBag.HasNewReviewForSeller = isStaffOrAdmin && newReviewsForSeller.Any();
        ViewBag.NewReviewCount = newReviewsForSeller.Count;
        ViewBag.LatestNewReview = newReviewsForSeller.FirstOrDefault();

        return View(shoes);
    }

    // GET: SHOESS/Create
    public IActionResult Create()
    {
        if (!IsAdmin() && !IsStaff())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        ViewBag.Categories = _context.Category.ToList();
        return View();
    }

    // POST: SHOESS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductId,ShoesName,Category,CategoryId,ShoesGender,ShoesSize,ShoesColor,Material,Description,Price,SalePercentage")] Shoes shoes, IFormFile imageFile)
    {
        if (!IsAdmin() && !IsStaff())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        if (ModelState.IsValid)
        {
            var colorCode = string.IsNullOrWhiteSpace(shoes.ShoesColor) ? "XXX" : shoes.ShoesColor.Substring(0, Math.Min(3, shoes.ShoesColor.Length)).ToUpper();
            shoes.SkuId = $"{shoes.CategoryId}-{shoes.ShoesGender.ToString().Substring(0, 1)}-{shoes.ShoesSize}-{colorCode}";

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                shoes.ImageUrl = "/images/" + uniqueFileName;
            }

            if (shoes.SalePercentage == null)
            {
                shoes.SalePercentage = 0;
            }

            _context.Add(shoes);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Categories = await _context.Category.ToListAsync();
        return View(shoes);
    }

    // GET: SHOESS/Edit/5
    public async Task<IActionResult> Edit(System.Guid? id)
    {
        if (!IsAdmin() && !IsStaff())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        if (id == null)
        {
            return NotFound();
        }

        var shoes = await _context.Shoes.FindAsync(id);
        if (shoes == null)
        {
            return NotFound();
        }
        return View(shoes);
    }

    // POST: SHOESS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(System.Guid? id, [Bind("ProductId,ShoesName,SkuId,Category,CategoryId,ShoesGender,ShoesSize,ShoesColor,Material,Description,Price,SalePercentage")] Shoes shoes, IFormFile imageFile)
    {
        var colorCode = string.IsNullOrWhiteSpace(shoes.ShoesColor) ? "XXX" : shoes.ShoesColor.Substring(0, Math.Min(3, shoes.ShoesColor.Length)).ToUpper();
        shoes.SkuId = $"{shoes.CategoryId}-{shoes.ShoesGender.ToString().Substring(0, 1)}-{shoes.ShoesSize}-{colorCode}";

        if (!IsAdmin() && !IsStaff())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        if (id != shoes.ProductId)
        {
            return NotFound();
        }

        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.Remove("imageFile");
        }

        if (!ModelState.IsValid)
        {
            return View(shoes);
        }

        var existingShoes = await _context.Shoes.FindAsync(id);
        if (existingShoes == null)
        {
            return NotFound();
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            existingShoes.ImageUrl = "/images/" + uniqueFileName;
        }

        existingShoes.ShoesName = shoes.ShoesName;
        existingShoes.SkuId = shoes.SkuId;
        existingShoes.CategoryId = shoes.CategoryId;
        existingShoes.ShoesGender = shoes.ShoesGender;
        existingShoes.ShoesSize = shoes.ShoesSize;
        existingShoes.ShoesColor = shoes.ShoesColor;
        existingShoes.Material = shoes.Material;
        existingShoes.Description = shoes.Description;
        existingShoes.Price = shoes.Price;
        existingShoes.SalePercentage = shoes.SalePercentage;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ShoesExists(shoes.ProductId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: SHOESS/Delete/5
    public async Task<IActionResult> Delete(System.Guid? id)
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        if (id == null)
        {
            return NotFound();
        }

        var shoes = await _context.Shoes
            .FirstOrDefaultAsync(m => m.ProductId == id);
        if (shoes == null)
        {
            return NotFound();
        }

        return View(shoes);
    }

    // POST: SHOESS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(System.Guid? id)
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path + Request.QueryString });
        var shoes = await _context.Shoes.FindAsync(id);
        if (shoes != null)
        {
            _context.Shoes.Remove(shoes);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }

    private bool ShoesExists(System.Guid? id)
    {
        return _context.Shoes.Any(e => e.ProductId == id);
    }
}
