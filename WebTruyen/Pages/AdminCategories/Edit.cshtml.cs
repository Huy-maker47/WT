using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminCategories
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
        public Category CategoryInput { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            CategoryInput = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.CategoryName = CategoryInput.CategoryName;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu: " + (ex.InnerException?.Message ?? ex.Message));
                return Page();
            }
        }
    }
}