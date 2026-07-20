using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebTruyenContext _context;

        public HomeController(WebTruyenContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.CreatedDate)
                .Take(8)
                .ToListAsync();

            return View(featuredBooks);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}