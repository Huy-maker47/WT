using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminOrdersController : Controller
    {
        private readonly WebTruyenContext _context;

        public AdminOrdersController(WebTruyenContext context)
        {
            _context = context;
        }

        // GET: /AdminOrders?status=Chờ xác nhận
        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            ViewBag.Status = status;
            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}