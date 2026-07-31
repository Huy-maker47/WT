using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Chat
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public int MyUserId { get; set; }
        public int AdminId { get; set; }
        public List<Message> Messages { get; set; } = new();

        public async Task OnGetAsync()
        {
            MyUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            AdminId = admin?.UserId ?? 0;

            Messages = await _context.Messages
                .Where(m => (m.SenderId == MyUserId && m.ReceiverId == AdminId)
                         || (m.SenderId == AdminId && m.ReceiverId == MyUserId))
                .OrderBy(m => m.SentDate)
                .ToListAsync();
        }

        public class SendMessageRequest
        {
            public int ReceiverId { get; set; }
            public string Content { get; set; } = "";
        }

        public async Task<IActionResult> OnPostSendMessageAsync(
     [FromBody] SendMessageRequest req)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var myUserId))
            {
                return Unauthorized();
            }

            if (req.ReceiverId <= 0 ||
                string.IsNullOrWhiteSpace(req.Content))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tin nhắn không hợp lệ."
                });
            }

            var receiverExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == req.ReceiverId &&
                    u.Role == "Admin" &&
                    u.IsActive == true);

            if (!receiverExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy Admin."
                });
            }

            var message = new Message
            {
                SenderId = myUserId,
                ReceiverId = req.ReceiverId,
                Content = req.Content.Trim(),
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);

            // Tạo thông báo cho Admin nhận tin
            _context.Notifications.Add(new Notification
            {
                UserId = req.ReceiverId,
                Title = "Có tin nhắn mới từ khách hàng",
                Content = req.Content.Trim(),
                Link = $"/AdminChat/Chat/{myUserId}",
                CreatedDate = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                messageId = message.MessageId
            });
        }
    }
}