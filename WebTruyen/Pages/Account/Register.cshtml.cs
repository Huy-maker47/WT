using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTruyen.Models;

namespace WebTruyen.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public RegisterModel(WebTruyenContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.Role");
            ModelState.Remove("Input.IsActive");
            ModelState.Remove("Input.CreatedDate");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_context.Users.Any(u => u.Username == Input.Username))
            {
                ModelState.AddModelError("", "Username đã tồn tại");
                return Page();
            }

            Input.Role = "Customer";
            Input.IsActive = true;
            Input.CreatedDate = DateTime.Now;

            _context.Users.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("Login");
        }
    }
}