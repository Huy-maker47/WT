using System;
using System.Collections.Generic;

namespace WebTruyen.Models
{
    public partial class Book
    {
        public Book()
        {
            CartItems = new HashSet<CartItem>();
            Favorites = new HashSet<Favorite>();
            OrderDetails = new HashSet<OrderDetail>();
            Reviews = new HashSet<Review>();
        }

        public int BookId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
