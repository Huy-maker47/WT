using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Cart
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public CheckoutModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Models.Cart CartData { get; set; } = null!;

        [BindProperty, Required]
        public string ReceiverName { get; set; } = "";

        [BindProperty, Required]
        public string ReceiverPhone { get; set; } = "";

        [BindProperty, Required]
        public string ShippingAddress { get; set; } = "";

        [BindProperty]
        public string PaymentMethod { get; set; } = "COD";

        [BindProperty]
        public string? Note { get; set; }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private async Task<Models.Cart?> GetCartAsync()
        {
            var userId = GetUserId();
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var cart = await GetCartAsync();
            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            CartData = cart;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var cart = await GetCartAsync();
            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            if (!ModelState.IsValid)
            {
                CartData = cart;
                return Page();
            }

            var order = new Order
            {
                UserId = GetUserId(),
                OrderDate = DateTime.Now,
                ReceiverName = ReceiverName,
                ReceiverPhone = ReceiverPhone,
                ShippingAddress = ShippingAddress,
                PaymentMethod = PaymentMethod,
                PaymentStatus = "Chưa thanh toán",
                Status = "Chờ xác nhận",
                Note = Note,
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Book.Price)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Cart/OrderSuccess", new { id = order.OrderId });
        }
    }
}