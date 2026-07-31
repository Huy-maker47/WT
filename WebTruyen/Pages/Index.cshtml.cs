using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Book> FeaturedBooks { get; set; } = new();

        public async Task OnGetAsync()
        {
            FeaturedBooks = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.CreatedDate)
                .Take(8)
                .ToListAsync();
        }
    }
}