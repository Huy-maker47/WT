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
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Details", new { id = orderId });
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
    }
}