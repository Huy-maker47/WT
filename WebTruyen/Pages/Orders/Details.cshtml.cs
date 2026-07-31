//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace WebTruyen.Pages.Orders
//{
//    public class DetailsModel : PageModel
//    {
//        public void OnGet()
//        {
//        }
//    }
//}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTruyen.Models;

namespace WebTruyen.Pages.Orders
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly WebTruyenContext _context;

        public DetailsModel(WebTruyenContext context)
        {
            _context = context;
        }

        public Order? MyOrder { get; set; }

        public async Task OnGetAsync(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            MyOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
            // vẫn giữ điều kiện UserId == userId để chống IDOR như bản MVC trước
        }
    }
}