using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

            // Xóa khoảng trắng thừa
            Input.Username = Input.Username.Trim();
            Input.Email = Input.Email.Trim().ToLower();

            // Kiểm tra trùng tên đăng nhập
            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == Input.Username);

            if (usernameExists)
            {
                ModelState.AddModelError(
                    "Input.Username",
                    "Tên đăng nhập đã tồn tại.");

                return Page();
            }

            // Kiểm tra trùng email
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == Input.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "Email này đã được sử dụng.");

                return Page();
            }

            Input.Role = "Customer";
            Input.IsActive = true;
            Input.CreatedDate = DateTime.Now;

            _context.Users.Add(Input);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc email đã tồn tại.");

                return Page();
            }

            TempData["Success"] =
                "Đăng ký thành công. Vui lòng đăng nhập.";

            return RedirectToPage("/Account/Login");
        }
    }
}//using Microsoft.AspNetCore.Mvc;
 //using Microsoft.AspNetCore.Mvc.RazorPages;
 //using WebTruyen.Models;

//namespace WebTruyen.Pages.Account
//{
//    public class RegisterModel : PageModel
//    {
//        private readonly WebTruyenContext _context;

//        public RegisterModel(WebTruyenContext context)
//        {
//            _context = context;
//        }

//        [BindProperty]
//        public User Input { get; set; } = new();

//        public void OnGet()
//        {
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            ModelState.Remove("Input.Role");
//            ModelState.Remove("Input.IsActive");
//            ModelState.Remove("Input.CreatedDate");

//            if (!ModelState.IsValid)
//            {
//                return Page();
//            }

//            if (_context.Users.Any(u => u.Username == Input.Username))
//            {
//                ModelState.AddModelError("", "Username đã tồn tại");
//                return Page();
//            }

//            Input.Role = "Customer";
//            Input.IsActive = true;
//            Input.CreatedDate = DateTime.Now;

//            _context.Users.Add(Input);
//            await _context.SaveChangesAsync();

//            return RedirectToPage("Login");
//        }
//    }
//}