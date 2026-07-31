using System;
using System.Collections.Generic;

namespace WebTruyen.Models
{
    public partial class Cart
    {
        public Cart()
        {
            CartItems = new HashSet<CartItem>();
        }

        public int CartId { get; set; }
        public int UserId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
