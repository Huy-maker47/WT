




using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    public class BooksController : Controller
    {
        private readonly WebTruyenContext _context;

        public BooksController(WebTruyenContext context)
        {
            _context = context;
        }

        // GET: /Books hoặc /Books/Index?categoryId=2&keyword=abc
        public async Task<IActionResult> Index(int? categoryId, string? keyword)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive == true)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(b => b.Title.Contains(keyword) || b.Author.Contains(keyword));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = categoryId;
            ViewBag.Keyword = keyword;

            var books = await query.OrderByDescending(b => b.CreatedDate).ToListAsync();
            return View(books);
        }

        // GET: /Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == id && b.IsActive == true);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
    }
}