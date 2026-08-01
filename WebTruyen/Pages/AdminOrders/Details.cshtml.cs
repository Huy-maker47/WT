
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminOrders
{
    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public DetailsModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Order OrderData { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            OrderData = order;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders
                .FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            bool isBankTransfer =
                order.PaymentMethod == "Chuyển khoản";

            bool wantsToProcessOrder =
                status == "Đang giao" ||
                status == "Hoàn thành";

            if (isBankTransfer &&
                order.PaymentStatus != "Đã thanh toán" &&
                wantsToProcessOrder)
            {
                TempData["Error"] =
                    "Đơn chuyển khoản chưa được xác nhận thanh toán nên chưa thể giao hàng.";

                return RedirectToPage(
                    "Details",
                    new { id = orderId });
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật trạng thái đơn hàng.";

            return RedirectToPage(
                "Details",
                new { id = orderId });
        }

        public async Task<IActionResult> OnPostUpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Details", new { id = orderId });
        }
        public async Task<IActionResult>
    OnPostConfirmBankTransferAsync(int orderId)
        {
            var order = await _context.Orders
                .FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentMethod != "Chuyển khoản")
            {
                TempData["Error"] =
                    "Đơn hàng này không sử dụng chuyển khoản.";

                return RedirectToPage(
                    "Details",
                    new { id = orderId });
            }

            if (order.PaymentStatus !=
                "Chờ xác nhận thanh toán")
            {
                TempData["Error"] =
                    "Đơn hàng không ở trạng thái chờ xác nhận.";

                return RedirectToPage(
                    "Details",
                    new { id = orderId });
            }

            order.PaymentStatus = "Đã thanh toán";

            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Thanh toán đã được xác nhận",
                Content =
                    $"Đơn hàng #{order.OrderId} đã được xác nhận thanh toán thành công.",
                Link = $"/Orders/Details/{order.OrderId}",
                CreatedDate = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã xác nhận thanh toán và gửi thông báo cho khách hàng.";

            return RedirectToPage(
                "Details",
                new { id = orderId });
        }

        public async Task<IActionResult>
    OnPostRejectBankTransferAsync(int orderId)
        {
            var order = await _context.Orders
                .FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentMethod != "Chuyển khoản")
            {
                return RedirectToPage(
                    "Details",
                    new { id = orderId });
            }

            order.PaymentStatus = "Chưa nhận được tiền";

            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Chưa xác nhận được thanh toán",
                Content =
                    $"METruyen chưa nhận được tiền của đơn hàng #{order.OrderId}. Vui lòng kiểm tra lại thông tin chuyển khoản.",
                Link = $"/Orders/Details/{order.OrderId}",
                CreatedDate = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật trạng thái và gửi thông báo cho khách hàng.";

            return RedirectToPage(
                "Details",
                new { id = orderId });
        }

    }
}