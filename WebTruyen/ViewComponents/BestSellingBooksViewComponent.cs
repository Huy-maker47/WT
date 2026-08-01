using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.ViewComponents
{
    public class BestSellingBooksViewComponent : ViewComponent
    {
        private readonly WebTruyenContext _context;

        public BestSellingBooksViewComponent(
            WebTruyenContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int count = 4)
        {
            var sales = await _context.OrderDetails
                .AsNoTracking()
                .GroupBy(detail => detail.BookId)
                .Select(group => new
                {
                    BookId = group.Key,
                    TotalSold = group.Sum(detail => detail.Quantity)
                })
                .OrderByDescending(item => item.TotalSold)
                .Take(count)
                .ToListAsync();

            var bookIds = sales
                .Select(item => item.BookId)
                .ToList();

            var books = await _context.Books
                .AsNoTracking()
                .Where(book =>
                    bookIds.Contains(book.BookId) &&
                    book.IsActive == true)
                .ToDictionaryAsync(book => book.BookId);

            var result = sales
                .Where(item => books.ContainsKey(item.BookId))
                .Select(item => new BestSellingBookItem
                {
                    Book = books[item.BookId],
                    TotalSold = item.TotalSold
                })
                .ToList();

            return View(result);
        }

        public class BestSellingBookItem
        {
            public Book Book { get; set; } = null!;

            public int TotalSold { get; set; }
        }
    }
}   