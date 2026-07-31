using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Hubs;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminChat
{
    [Authorize(Roles = "Admin,Staff")]
    public class ChatModel : PageModel
    {
        private readonly WebTruyenContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatModel(
            WebTruyenContext context,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public int MyUserId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = "";

        public List<Message> Messages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var myUserId))
            {
                return Unauthorized();
            }

            MyUserId = myUserId;
            CustomerId = userId;

            var customer = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            CustomerName = customer.FullName;

            Messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == userId &&
                     m.ReceiverId == MyUserId)
                    ||
                    (m.SenderId == MyUserId &&
                     m.ReceiverId == userId))
                .OrderBy(m => m.SentDate)
                .ToListAsync();

            var unreadMessages = Messages
                .Where(m =>
                    m.SenderId == userId &&
                    m.ReceiverId == MyUserId &&
                    m.IsRead == false)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public class SendMessageRequest
        {
            public int ReceiverId { get; set; }

            public string Content { get; set; } = "";
        }

        public async Task<IActionResult> OnPostSendMessageAsync(
            [FromBody] SendMessageRequest request)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var myUserId))
            {
                return Unauthorized();
            }

            var content = request.Content?.Trim();

            if (request.ReceiverId <= 0 ||
                string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nội dung tin nhắn không hợp lệ."
                });
            }

            var receiverExists = await _context.Users
                .AnyAsync(u => u.UserId == request.ReceiverId);

            if (!receiverExists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy người nhận."
                });
            }

            var message = new Message
            {
                SenderId = myUserId,
                ReceiverId = request.ReceiverId,
                Content = content,
                SentDate = DateTime.Now,
                IsRead = false
            }; 
            _context.Messages.Add(message);

            _context.Notifications.Add(new Notification
            {
                UserId = request.ReceiverId,
                Title = "Admin vừa trả lời tin nhắn",
                Content = content,
                Link = "/Chat",
                CreatedDate = DateTime.Now,
                IsRead = false
            });


            await _context.SaveChangesAsync();

            // Server chủ động gửi realtime cho người dùng.
            await _hubContext.Clients
                .Group($"user_{request.ReceiverId}")
                .SendAsync("ReceiveMessage", content);

            return new JsonResult(new
            {
                success = true,
                messageId = message.MessageId
            });
        }
    }
}