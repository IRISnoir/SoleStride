
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

public class CategoryController : Controller
{
    private readonly SoleStrideDbContext _context;
    public CategoryController(SoleStrideDbContext context)
    {
        _context = context;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("Role") == "Admin";
    }

    // GET: CATEGORYS
    public async Task<IActionResult> Index()    
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");
        return View(await _context.Category.ToListAsync());
    }

    // GET: CATEGORYS/Details/5
    public async Task<IActionResult> Details(string? categoryid)
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");

        if (categoryid == null)
        {
            return NotFound();
        }

        var category = await _context.Category
            .FirstOrDefaultAsync(m => m.CategoryId == categoryid);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // GET: CATEGORYS/Create
    public async Task<IActionResult> Create()
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");
        ViewBag.Categories = await _context.Category.ToListAsync();
        return View(new Category());
    }

    // POST: CATEGORYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryId,CategoryName")] Category category)
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create));
        }
        ViewBag.Categories = await _context.Category.ToListAsync();
        return View(category);
    }


    // GET: CATEGORYS/Delete/5
    public async Task<IActionResult> Delete(string? categoryid)
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");
        if (categoryid == null)
        {
            return NotFound();
        }

        var category = await _context.Category
            .FirstOrDefaultAsync(m => m.CategoryId == categoryid);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: CATEGORYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? categoryid)
    {
        if (!IsAdmin())
            return RedirectToAction("Index", "Home");
        var category = await _context.Category.FindAsync(categoryid);
        if (category != null)
        {
            _context.Category.Remove(category);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Create));
    }

    private bool CategoryExists(string? categoryid)
    {
        return _context.Category.Any(e => e.CategoryId == categoryid);
    }
}
