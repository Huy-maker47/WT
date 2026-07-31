using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.Books
{
    public class BestSellersModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public BestSellersModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<BestSellerItem> BestSellingBooks { get; set; } = new();

        public class BestSellerItem
        {
            public Book Book { get; set; } = null!;

            public int SoldQuantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Cộng tổng số lượng đã đặt của từng cuốn sách.
            var sales = await _context.OrderDetails
                .Where(od => od.Book.IsActive == true)
                .GroupBy(od => od.BookId)
                .Select(group => new
                {
                    BookId = group.Key,
                    SoldQuantity = group.Sum(od => od.Quantity)
                })
                .OrderByDescending(item => item.SoldQuantity)
                .Take(24)
                .ToListAsync();

            var bookIds = sales
                .Select(item => item.BookId)
                .ToList();

            var books = await _context.Books
                .Include(book => book.Category)
                .Where(book =>
                    bookIds.Contains(book.BookId) &&
                    book.IsActive == true)
                .ToDictionaryAsync(book => book.BookId);

            BestSellingBooks = sales
                .Where(item => books.ContainsKey(item.BookId))
                .Select(item => new BestSellerItem
                {
                    Book = books[item.BookId],
                    SoldQuantity = item.SoldQuantity
                })
                .ToList();
        }
    }
}