using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Cart
{
    [Authorize]
    public class BankTransferModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public BankTransferModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Order OrderData { get; set; } = null!;

        // Thông tin demo, đổi thành tài khoản thật của nhóm.
        public string BankName { get; } = "MB Bank";

        public string AccountNumber { get; } = "0123456789";

        public string AccountHolder { get; } = "METRUYEN";

        public string TransferContent =>
            $"METRUYEN ORD{OrderData.OrderId}";

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }

        private async Task<Order?> GetOrderAsync(int id)
        {
            int userId = GetUserId();

            return await _context.Orders
                .Include(order => order.OrderDetails)
                    .ThenInclude(detail => detail.Book)
                .FirstOrDefaultAsync(order =>
                    order.OrderId == id &&
                    order.UserId == userId &&
                    order.PaymentMethod == "Chuyển khoản");
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            OrderData = order;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var order = await GetOrderAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Khách bấm xác nhận không đồng nghĩa tiền đã vào.
            if (order.PaymentStatus != "Đã thanh toán")
            {
                order.PaymentStatus =
                    "Chờ xác nhận thanh toán";

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(
                "/Cart/OrderSuccess",
                new { id = order.OrderId });
        }
    }
}