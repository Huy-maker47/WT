using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace WebTruyen.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Gửi tin nhắn realtime tới người nhận cụ thể (theo UserId)
        public async Task SendMessage(int receiverId, string content)
        {
            await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", content);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }
    }
}