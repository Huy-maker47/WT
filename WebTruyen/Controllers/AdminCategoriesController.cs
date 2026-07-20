using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminCategoriesController : Controller
    {
        private readonly WebTruyenContext _context;

        public AdminCategoriesController(WebTruyenContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + (ex.InnerException?.Message ?? ex.Message));
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.CategoryId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.CategoryName = model.CategoryName;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + (ex.InnerException?.Message ?? ex.Message));
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            if (category.Books.Any())
            {
                TempData["Error"] = "Không thể xóa danh mục đang có sách!";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}