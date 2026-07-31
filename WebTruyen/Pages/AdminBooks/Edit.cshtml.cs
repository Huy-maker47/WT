using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminBooks
{
    [Authorize(Roles = "Admin,Staff")]
    public class EditModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public EditModel(WebTruyenContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book BookInput { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            BookInput = book;
            Categories = await _context.Categories.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ModelState.Remove("BookInput.CreatedDate");
            ModelState.Remove("BookInput.IsActive");
            ModelState.Remove("BookInput.Category");
            ModelState.Remove("BookInput.ImageUrl");

            if (!ModelState.IsValid)
            {
                Categories = await _context.Categories.ToListAsync();
                return Page();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = BookInput.Title;
            book.Author = BookInput.Author;
            book.Description = BookInput.Description;
            book.Price = BookInput.Price;
            book.Stock = BookInput.Stock;
            book.CategoryId = BookInput.CategoryId;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                book.ImageUrl = "/images/books/" + fileName;
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("", "Lỗi khi lưu: " + (ex.InnerException?.Message ?? ex.Message));
                return Page();
            }
        }
    }
}