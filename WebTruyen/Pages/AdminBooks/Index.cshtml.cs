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
            Books = await _context.Books
                .Include(book => book.Category)
                .OrderByDescending(book => book.IsActive == true)
                .ThenByDescending(
                    book => book.UpdatedDate ?? book.CreatedDate
                )
                .ThenByDescending(book => book.BookId)
                .ToListAsync();
        }

        public async Task<IActionResult>
            OnPostToggleActiveAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                TempData["Error"] =
                    "Không tìm thấy sách cần cập nhật.";

                return RedirectToPage();
            }

            book.IsActive = !(book.IsActive ?? true);
            book.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = book.IsActive == true
                ? $"Đã hiển thị sách \"{book.Title}\"."
                : $"Đã ẩn sách \"{book.Title}\".";

            return RedirectToPage();
        }

        public async Task<IActionResult>
            OnPostDeleteAsync(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(book =>
                    book.BookId == id
                );

            if (book == null)
            {
                TempData["Error"] =
                    "Không tìm thấy sách cần xóa.";

                return RedirectToPage();
            }

            /*
             * Không xóa sách đã từng được mua,
             * vì OrderDetail cần giữ lại lịch sử đơn hàng.
             */
            bool existsInOrder =
                await _context.OrderDetails
                    .AnyAsync(detail =>
                        detail.BookId == id
                    );

            if (existsInOrder)
            {
                TempData["Error"] =
                    $"Không thể xóa \"{book.Title}\" vì sách đã tồn tại trong đơn hàng. Hãy dùng chức năng Ẩn.";

                return RedirectToPage();
            }

            /*
             * Xóa các dữ liệu phụ trước khi xóa sách.
             */
            var cartItems =
                await _context.CartItems
                    .Where(item =>
                        item.BookId == id
                    )
                    .ToListAsync();

            var favorites =
                await _context.Favorites
                    .Where(item =>
                        item.BookId == id
                    )
                    .ToListAsync();

            var recentlyViewed =
                await _context.RecentlyViewedBooks
                    .Where(item =>
                        item.BookId == id
                    )
                    .ToListAsync();

            var reviews =
                await _context.Reviews
                    .Where(item =>
                        item.BookId == id
                    )
                    .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            _context.Favorites.RemoveRange(favorites);
            _context.RecentlyViewedBooks.RemoveRange(
                recentlyViewed
            );
            _context.Reviews.RemoveRange(reviews);

            _context.Books.Remove(book);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Đã xóa sách \"{book.Title}\".";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa sách vì sách vẫn đang được sử dụng bởi dữ liệu khác.";
            }

            return RedirectToPage();
        }
    }
}