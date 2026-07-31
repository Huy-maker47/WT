using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTruyen.Models;

namespace WebTruyen.Pages.AdminCategories
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
        public Category CategoryInput { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.Categories.Add(CategoryInput);
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