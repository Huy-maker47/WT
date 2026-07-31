using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.RecentlyViewed
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<RecentlyViewedBook> ViewedBooks { get; set; } = new();

        private int? GetUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdText, out int userId))
            {
                return userId;
            }

            return null;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            ViewedBooks = await _context.RecentlyViewedBooks
                .Include(x => x.Book)
                .Where(x =>
                    x.UserId == userId.Value &&
                    x.Book.IsActive == true)
                .OrderByDescending(x => x.ViewedDate)
                .Take(30)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var history = await _context.RecentlyViewedBooks
                .Where(x => x.UserId == userId.Value)
                .ToListAsync();

            if (history.Count > 0)
            {
                _context.RecentlyViewedBooks.RemoveRange(history);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Đã xóa lịch sử sách đã xem.";

            return RedirectToPage();
        }
    }
}