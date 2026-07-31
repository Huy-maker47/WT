using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminChat
{
    [Authorize(Roles = "Admin,Staff")]
    public class ChatModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public ChatModel(WebTruyenContext context)
        {
            _context = context;
        }

        public int MyUserId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public List<Message> Messages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            MyUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            CustomerId = userId;

            var customer = await _context.Users.FindAsync(userId);
            if (customer == null) return NotFound();

            CustomerName = customer.FullName;

            Messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == MyUserId)
                         || (m.SenderId == MyUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SentDate)
                .ToListAsync();

            // Đánh dấu đã đọc các tin nhắn từ Customer gửi tới mình
            var unread = Messages.Where(m => m.SenderId == userId && m.IsRead == false).ToList();
            foreach (var m in unread)
            {
                m.IsRead = true;
            }
            if (unread.Any())
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

        public async Task<IActionResult> OnPostSendMessageAsync([FromBody] SendMessageRequest req)
        {
            var myUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _context.Messages.Add(new Message
            {
                SenderId = myUserId,
                ReceiverId = req.ReceiverId,
                Content = req.Content,
                SentDate = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}