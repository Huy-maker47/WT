using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminBooks
{
    [Authorize(Roles = "Admin,Staff")]
    public class CreateModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public CreateModel(WebTruyenContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book BookInput { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("BookInput.UpdatedDate");
            ModelState.Remove("BookInput.CreatedDate");
            ModelState.Remove("BookInput.IsActive");
            ModelState.Remove("BookInput.Category");
            ModelState.Remove("BookInput.ImageUrl");

            if (!ModelState.IsValid)
            {
                Categories = await _context.Categories.ToListAsync();
                return Page();
            }

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

                BookInput.ImageUrl = "/images/books/" + fileName;
            }

            BookInput.CreatedDate = DateTime.Now;
BookInput.UpdatedDate = DateTime.Now;
BookInput.IsActive = true;

            try
            {
                _context.Books.Add(BookInput);
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