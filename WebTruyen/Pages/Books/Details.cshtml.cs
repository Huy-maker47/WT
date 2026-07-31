using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Books
{
    public class DetailsModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public DetailsModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Book? Book { get; set; }
        public List<Review> Reviews { get; set; } = new();
        public bool IsFavorite { get; set; }
        public double AverageRating { get; set; }

        [BindProperty]
        public int RatingInput { get; set; }

        [BindProperty]
        public string? CommentInput { get; set; }

        private int? GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim) : null;
        }

        public async Task OnGetAsync(int id)
        {
            await LoadDataAsync(id);
        }

        private async Task LoadDataAsync(int id)
        {
            Book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == id && b.IsActive == true);

            if (Book == null) return;

            Reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            AverageRating = Reviews.Any() ? Reviews.Average(r => r.Rating ?? 0) : 0;
            RelatedBooks = await _context.Books
    .Where(b => b.CategoryId == Book.CategoryId && b.BookId != Book.BookId && b.IsActive == true)
    .OrderByDescending(b => b.CreatedDate)
    .Take(4)
    .ToListAsync();
            var userId = GetUserId();
            if (userId.HasValue)
            {
                IsFavorite = await _context.Favorites
                    .AnyAsync(f => f.UserId == userId.Value && f.BookId == id);
            }
        }

        // POST: ?handler=AddReview
        public async Task<IActionResult> OnPostAddReviewAsync(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            if (RatingInput < 1 || RatingInput > 5)
            {
                ModelState.AddModelError("", "Vui lòng chọn số sao từ 1 đến 5");
                await LoadDataAsync(id);
                return Page();
            }

            // Mỗi user chỉ đánh giá 1 lần / sách -> nếu đã có thì update thay vì thêm mới
            var existing = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId.Value && r.BookId == id);

            if (existing != null)
            {
                existing.Rating = RatingInput;
                existing.Comment = CommentInput;
                existing.ReviewDate = DateTime.Now;
            }
            else
            {
                _context.Reviews.Add(new Review
                {
                    UserId = userId.Value,
                    BookId = id,
                    Rating = RatingInput,
                    Comment = CommentInput,
                    ReviewDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }

        // POST: ?handler=ToggleFavorite
        public async Task<IActionResult> OnPostToggleFavoriteAsync(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            var fav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.BookId == id);

            if (fav != null)
            {
                _context.Favorites.Remove(fav);
            }
            else
            {
                _context.Favorites.Add(new Favorite { UserId = userId.Value, BookId = id });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }
        public List<Book> RelatedBooks { get; set; } = new();
    }
}