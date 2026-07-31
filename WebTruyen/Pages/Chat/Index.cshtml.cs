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
[Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]

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