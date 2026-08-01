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

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .OrderBy(category => category.CategoryName)
                .ToListAsync();

            var query = _context.Books
                .AsNoTracking()
                .Include(book => book.Category)
                .Where(book => book.IsActive == true)
                .AsQueryable();

            // Lọc theo danh mục
            if (CategoryId.HasValue)
            {
                query = query.Where(book =>
                    book.CategoryId == CategoryId.Value
                );
            }

            // Lọc chính xác theo tác giả khi bấm tên tác giả
            if (!string.IsNullOrWhiteSpace(AuthorName))
            {
                query = query.Where(book =>
                    book.Author == AuthorName
                );
            }
            // Tìm gần đúng theo tên sách hoặc tác giả
            else if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string keyword = Keyword.Trim();

                query = query.Where(book =>
                    book.Title.Contains(keyword) ||
                    book.Author.Contains(keyword)
                );
            }

            // Lọc giá tối thiểu
            if (MinPrice.HasValue)
            {
                query = query.Where(book =>
                    book.Price >= MinPrice.Value
                );
            }

            // Lọc giá tối đa
            if (MaxPrice.HasValue)
            {
                query = query.Where(book =>
                    book.Price <= MaxPrice.Value
                );
            }

            // Sắp xếp
            query = Sort switch
            {
                "title_az" => query
                    .OrderBy(book => book.Title),

                "title_za" => query
                    .OrderByDescending(book => book.Title),

                "price_asc" => query
                    .OrderBy(book => book.Price),

                "price_desc" => query
                    .OrderByDescending(book => book.Price),

                _ => query
                    .OrderByDescending(book => book.CreatedDate)
                    .ThenByDescending(book => book.BookId)
            };

            // Đếm tổng số sách sau khi lọc
            TotalItems = await query.CountAsync();

            TotalPages = (int)Math.Ceiling(
                TotalItems / (double)PageSize
            );

            // Không cho số trang nhỏ hơn 1
            if (PageNumber < 1)
            {
                PageNumber = 1;
            }

            // Không cho vượt quá trang cuối
            if (TotalPages > 0 && PageNumber > TotalPages)
            {
                PageNumber = TotalPages;
            }

            Books = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}