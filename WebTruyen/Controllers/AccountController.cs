using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebTruyenContext _context;

        public AccountController(WebTruyenContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Staff")]
        public class AdminController : Controller
        {
            public IActionResult Index()
            {
                return View();
            }
        }
        

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username 
                                   && u.PasswordHash == password 
                                   && u.IsActive == true);

            if (user == null)
            {
                ModelState.AddModelError("", "Sai tài kho?n ho?c m?t kh?u");
                return View();
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
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "AdminBooks");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "Username đã tồn tại");
                return View(model);
            }

            model.Role = "Customer";
            model.IsActive = true;
            model.CreatedDate = DateTime.Now;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
[Authorize]
[HttpGet]
public async Task<IActionResult> Profile()
{
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userIdClaim))
    {
        return RedirectToAction("Login");
    }

    int userId = int.Parse(userIdClaim);

    var user = await _context.Users.FindAsync(userId);

    if (user == null)
    {
        return NotFound();
    }

    return View(user);
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Profile(
    string fullName,
    string email,
    string? phone,
    string? address,
    string? avatar)
{
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userIdClaim))
    {
        return RedirectToAction("Login");
    }

    int userId = int.Parse(userIdClaim);

    var user = await _context.Users.FindAsync(userId);

    if (user == null)
    {
        return NotFound();
    }

    if (_context.Users.Any(u => u.Email == email && u.UserId != userId))
    {
        ModelState.AddModelError("", "Email này đã được sử dụng.");
        return View(user);
    }

    user.FullName = fullName;
    user.Email = email;
    user.Phone = phone;
    user.Address = address;
    user.Avatar = avatar;

    await _context.SaveChangesAsync();

    ViewBag.Success = "Cập nhật thông tin thành công.";

    return View(user);
}

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}