using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    [Authorize] // chỉ cần đăng nhập, không cần Role cụ thể
    public class OrdersController : Controller
    {
        private readonly WebTruyenContext _context;

        public OrdersController(WebTruyenContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // GET: /Orders  -> danh sách đơn hàng của user đang đăng nhập
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
            // điều kiện UserId == userId RẤT quan trọng
            // để tránh user A xem được đơn của user B chỉ bằng cách đổi id trên URL

            if (order == null) return NotFound();

            return View(order);
        }
    }
}