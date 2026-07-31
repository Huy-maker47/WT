using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.Books
{
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Book> Books { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AuthorName { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive == true)
                .AsQueryable();

            if (CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(AuthorName))
            {
                // Bấm từ link tên tác giả -> so khớp chính xác
                query = query.Where(b => b.Author == AuthorName);
            }
            else if (!string.IsNullOrEmpty(Keyword))
            {
                // Ô tìm kiếm chung -> so khớp gần đúng cả Title lẫn Author
                query = query.Where(b => b.Title.Contains(Keyword) || b.Author.Contains(Keyword));
            }

            Books = await query.OrderByDescending(b => b.CreatedDate).ToListAsync();
        }
    }
}