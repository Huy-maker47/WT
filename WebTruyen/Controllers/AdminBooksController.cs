using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTruyen.Models;

namespace WebTruyen.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminBooksController : Controller
    {
        private readonly WebTruyenContext _context;

        public AdminBooksController(WebTruyenContext context)
        {
            _context = context;
        }

        // GET: /AdminBooks
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.Category).ToListAsync();
            return View(books);
        }

        // GET: /AdminBooks/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // POST: /AdminBooks/Create
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Create(Book model, IFormFile? imageFile)
        {
            ModelState.Remove("CreatedDate");
            ModelState.Remove("IsActive");
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl"); // vì giờ lấy từ file upload, không phải input text

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            // Xử lý upload ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/images/books/" + fileName;
            }

            model.CreatedDate = DateTime.Now;
            model.IsActive = true;

            try
            {
                _context.Books.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                var message = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Lỗi khi lưu: " + message);
                return View(model);
            }
        }

        // GET: /AdminBooks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(book);
        }

        // POST: /AdminBooks/Edit/5
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Book model, IFormFile? imageFile)
        {
            if (id != model.BookId) return NotFound();

            ModelState.Remove("CreatedDate");
            ModelState.Remove("IsActive");
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = model.Title;
            book.Author = model.Author;
            book.Description = model.Description;
            book.Price = model.Price;
            book.Stock = model.Stock;
            book.CategoryId = model.CategoryId;

            // Chỉ đổi ảnh nếu Admin có chọn file mới
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                book.ImageUrl = "/images/books/" + fileName;
            }
            // nếu không chọn ảnh mới -> giữ nguyên book.ImageUrl cũ

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                var message = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Lỗi khi lưu: " + message);
                return View(model);
            }
        }

        // POST: /AdminBooks/ToggleActive/5  (ẩn/hiện thay vì xóa hẳn)
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.IsActive = !(book.IsActive ?? true);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}