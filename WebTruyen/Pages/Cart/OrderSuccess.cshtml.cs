using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTruyen.Pages.Cart
{
    public class OrderSuccessModel : PageModel
    {
        public int OrderId { get; set; }

        public void OnGet(int id)
        {
            OrderId = id;
        }
    }
}