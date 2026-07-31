using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminBooks
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; } = new();

        public async Task OnGetAsync()
        {
            Books = await _context.Books.Include(b => b.Category).ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.IsActive = !(book.IsActive ?? true);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}