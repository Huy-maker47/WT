using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.ViewComponents
{
    public class NewUpdatedBooksViewComponent : ViewComponent
    {
        private readonly WebTruyenContext _context;

        public NewUpdatedBooksViewComponent(
            WebTruyenContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int count = 4)
        {
            var books = await _context.Books
                .AsNoTracking()
                .Where(book => book.IsActive == true)
                .OrderByDescending(
                    book => book.UpdatedDate ?? book.CreatedDate)
                .Take(count)
                .ToListAsync();

            return View(books);
        }
    }
}