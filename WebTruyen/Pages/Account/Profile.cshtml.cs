using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly WebTruyenContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(
            WebTruyenContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public ProfileInput Input { get; set; } = new();

        public string Username { get; set; } = "";

        public string Role { get; set; } = "";

        public string? CurrentAvatar { get; set; }

        public DateTime? CreatedDate { get; set; }

        public class ProfileInput
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [StringLength(
                100,
                ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
            public string FullName { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
            [StringLength(
                150,
                ErrorMessage = "Email không được vượt quá 150 ký tự.")]
            public string Email { get; set; } = "";

            [StringLength(
                20,
                ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
            public string? Phone { get; set; }

            [StringLength(
                255,
                ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
            public string? Address { get; set; }

            public IFormFile? AvatarFile { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            LoadProfile(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra email đã được tài khoản khác sử dụng chưa.
            var email = Input.Email.Trim();

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == email &&
                    u.UserId != userId.Value);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Input.Email",
                    "Email này đã được tài khoản khác sử dụng.");
            }

            // Kiểm tra file ảnh trước khi lưu.
            if (Input.AvatarFile != null)
            {
                ValidateAvatar(Input.AvatarFile);
            }

            if (!ModelState.IsValid)
            {
                LoadDisplayInformation(user);
                return Page();
            }

            user.FullName = Input.FullName.Trim();
            user.Email = email;
            user.Phone = Input.Phone?.Trim();
            user.Address = Input.Address?.Trim();

            if (Input.AvatarFile != null)
            {
                user.Avatar = await SaveAvatarAsync(
                    Input.AvatarFile,
                    user.UserId);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật thông tin cá nhân thành công.";

            return RedirectToPage();
        }

        private int? GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdText, out var userId))
            {
                return userId;
            }

            return null;
        }

        private void LoadProfile(User user)
        {
            Input = new ProfileInput
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone,
                Address = user.Address
            };

            LoadDisplayInformation(user);
        }

        private void LoadDisplayInformation(User user)
        {
            Username = user.Username;
            Role = user.Role ?? "Customer";
            CurrentAvatar = user.Avatar;
            CreatedDate = user.CreatedDate;
        }

        private void ValidateAvatar(IFormFile file)
        {
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "Input.AvatarFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            const long maximumSize = 2 * 1024 * 1024;

            if (file.Length > maximumSize)
            {
                ModelState.AddModelError(
                    "Input.AvatarFile",
                    "Ảnh đại diện không được vượt quá 2 MB.");
            }

            if (file.Length == 0)
            {
                ModelState.AddModelError(
                    "Input.AvatarFile",
                    "File ảnh không hợp lệ.");
            }
        }

        private async Task<string> SaveAvatarAsync(
            IFormFile file,
            int userId)
        {
            var avatarFolder = Path.Combine(
                _environment.WebRootPath,
                "images",
                "avatars");

            Directory.CreateDirectory(avatarFolder);

            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            var fileName =
                $"{userId}_{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(
                avatarFolder,
                fileName);

            await using var stream =
                new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            return $"/images/avatars/{fileName}";
        }
    }
}