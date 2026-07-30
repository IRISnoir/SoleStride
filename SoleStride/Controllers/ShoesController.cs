
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

    // GET: SHOESS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Shoes.Include(s => s.Category).ToListAsync());
    }

    // GET: SHOESS/Details/5
    public async Task<IActionResult> Details(System.Guid? id)
    {
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

    // GET: SHOESS/Create
    public IActionResult Create()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff")
            return RedirectToAction("Index", "Home");
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
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff")
            return RedirectToAction("Index", "Home");
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
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff")
            return RedirectToAction("Index", "Home");
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

        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin" && role != "Staff")
            return RedirectToAction("Index", "Home");
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
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
            return RedirectToAction("Index", "Home");
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
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
            return RedirectToAction("Index", "Home");
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
