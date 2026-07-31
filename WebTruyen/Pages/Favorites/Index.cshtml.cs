using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Favorites
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public IndexModel(WebTruyenContext context)
        {
            _context = context;
        }

        public List<Book> FavoriteBooks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            FavoriteBooks = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                .Select(f => f.Book)
                .ToListAsync();
        }
    }
}