using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public LoginModel(WebTruyenContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        public string Username { get; set; } = "";

        [BindProperty]
        [Required]
        public string Password { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = _context.Users
    .FirstOrDefault(u =>
        u.Username == Username &&
        u.PasswordHash == Password
    );

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Sai tài khoản hoặc mật khẩu."
                );

                return Page();
            }

            if (user.IsActive != true)
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin để được hỗ trợ."
                );

                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    principal,
    new AuthenticationProperties { IsPersistent = true });

            // Điều hướng theo Role
            if (user.Role == "Admin" || user.Role == "Staff")
            {
                return RedirectToPage("/AdminBooks/Index");
            }

            return RedirectToPage("/Index");
        }
    }
}