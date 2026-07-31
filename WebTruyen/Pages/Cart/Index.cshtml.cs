using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Models.Cart CartData { get; set; } = null!;

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private async Task<Models.Cart> GetOrCreateCartAsync()
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Models.Cart { UserId = userId, CreatedDate = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task OnGetAsync()
        {
            CartData = await GetOrCreateCartAsync();
        }

        // POST: /Cart?handler=AddToCart
        public async Task<IActionResult> OnPostAddToCartAsync(int bookId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();

            var item = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);
            if (item != null)
            {
                item.Quantity += quantity;
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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}