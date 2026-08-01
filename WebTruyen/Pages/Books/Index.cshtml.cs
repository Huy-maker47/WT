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

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => b.IsActive == true)
                .AsQueryable();

            // Lọc thể loại
            if (CategoryId.HasValue)
            {
                query = query.Where(
                    b => b.CategoryId == CategoryId.Value);
            }

            // Lọc theo tác giả
            if (!string.IsNullOrWhiteSpace(AuthorName))
            {
                query = query.Where(
                    b => b.Author == AuthorName);
            }
            // Tìm gần đúng theo tên sách hoặc tác giả
            else if (!string.IsNullOrWhiteSpace(Keyword))
            {
                query = query.Where(
                    b => b.Title.Contains(Keyword) ||
                         b.Author.Contains(Keyword));
            }

            // Nếu nhập giá nhỏ nhất lớn hơn giá lớn nhất thì đổi lại
            if (MinPrice.HasValue &&
                MaxPrice.HasValue &&
                MinPrice.Value > MaxPrice.Value)
            {
                (MinPrice, MaxPrice) = (MaxPrice, MinPrice);
            }

            // Lọc giá tối thiểu
            if (MinPrice.HasValue)
            {
                query = query.Where(
                    b => b.Price >= MinPrice.Value);
            }

            // Lọc giá tối đa
            if (MaxPrice.HasValue)
            {
                query = query.Where(
                    b => b.Price <= MaxPrice.Value);
            }

            // Sắp xếp
            query = Sort switch
            {
                "title_az" =>
                    query.OrderBy(b => b.Title),

                "title_za" =>
                    query.OrderByDescending(b => b.Title),

                "price_asc" =>
                    query.OrderBy(b => b.Price),

                "price_desc" =>
                    query.OrderByDescending(b => b.Price),

                _ =>
                    query.OrderByDescending(b => b.CreatedDate)
            };

            Books = await query.ToListAsync();
        }
    }
}