using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminChat
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public class CustomerChatInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; } = "";
            public string FullName { get; set; } = "";
            public string? LastMessage { get; set; }
            public DateTime? LastMessageDate { get; set; }
            public int UnreadCount { get; set; }
        }

        public List<CustomerChatInfo> Customers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var myUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Lấy tất cả user (không phải Admin/Staff) từng nhắn hoặc nhận tin với mình
            var relatedUserIds = await _context.Messages
                .Where(m => m.SenderId == myUserId || m.ReceiverId == myUserId)
                .Select(m => m.SenderId == myUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            foreach (var uid in relatedUserIds)
            {
                var user = await _context.Users.FindAsync(uid);
                if (user == null) continue;

                var lastMsg = await _context.Messages
                    .Where(m => (m.SenderId == uid && m.ReceiverId == myUserId)
                             || (m.SenderId == myUserId && m.ReceiverId == uid))
                    .OrderByDescending(m => m.SentDate)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderId == uid && m.ReceiverId == myUserId && m.IsRead == false);

                Customers.Add(new CustomerChatInfo
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    LastMessage = lastMsg?.Content,
                    LastMessageDate = lastMsg?.SentDate,
                    UnreadCount = unreadCount
                });
            }

            Customers = Customers.OrderByDescending(c => c.LastMessageDate).ToList();
        }
    }
}