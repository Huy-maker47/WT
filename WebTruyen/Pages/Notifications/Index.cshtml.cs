using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Notification>
    NotificationList
        { get; set; } = new();

        public int UnreadCount { get; set; }

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

        public async Task<IActionResult>
            OnGetAsync()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            NotificationList = await _context.Notifications
            .Where(n => n.UserId == userId.Value)
            .OrderByDescending(n => n.CreatedDate)
            .Take(50)
            .ToListAsync();

            UnreadCount = NotificationList.Count(n => !n.IsRead);

            return Page();
        }

        public async Task<IActionResult>
            OnPostMarkReadAsync(int id)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
            n.NotificationId == id &&
            n.UserId == userId.Value);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult>
            OnPostMarkAllReadAsync()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var notifications = await _context.Notifications
            .Where(n =>
            n.UserId == userId.Value &&
            n.IsRead == false)
            .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult>
            OnPostOpenAsync(int id)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
            n.NotificationId == id &&
            n.UserId == userId.Value);

            if (notification == null)
            {
                return RedirectToPage();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(notification.Link) &&
            Url.IsLocalUrl(notification.Link))
            {
                return LocalRedirect(notification.Link);
            }

            return RedirectToPage();
        }
    }
}
