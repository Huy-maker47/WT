using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.Books
{
    public class NewUpdatedModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public NewUpdatedModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; } = new();

        public async Task OnGetAsync()
        {
            Books = await _context.Books
                .Include(book => book.Category)
                .Where(book => book.IsActive == true)
                .OrderByDescending(
                    book => book.UpdatedDate ?? book.CreatedDate)
                .Take(24)
                .ToListAsync();
        }
    }
}