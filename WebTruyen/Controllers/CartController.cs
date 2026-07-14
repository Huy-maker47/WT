using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào Cart
    public class CartController : Controller
    {
        private readonly WebTruyenContext _context;

        public CartController(WebTruyenContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // Lấy hoặc tạo Cart cho user hiện tại
        private async Task<Cart> GetOrCreateCartAsync()
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedDate = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartAsync();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();

            var item = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);
            if (item != null)
            {
                item.Quantity += quantity; // đã có -> cộng dồn
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetOrCreateCartAsync();
            if (!cart.CartItems.Any())
            {
                return RedirectToAction("Index");
            }
            return View(cart);
        }

        // POST: /Cart/PlaceOrder
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string receiverName, string receiverPhone, string shippingAddress, string paymentMethod, string? note)
        {
            var cart = await GetOrCreateCartAsync();
            if (!cart.CartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserId = GetUserId(),
                OrderDate = DateTime.Now,
                ReceiverName = receiverName,
                ReceiverPhone = receiverPhone,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Chưa thanh toán",
                Status = "Chờ xác nhận",
                Note = note,
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Book.Price)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // cần SaveChanges trước để có OrderId

            foreach (var item in cart.CartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Book.Price
                });
            }

            // Xóa giỏ hàng sau khi đặt xong
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderSuccess", new { id = order.OrderId });
        }

        public IActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}