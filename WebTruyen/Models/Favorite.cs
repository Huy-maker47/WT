using System;
using System.Collections.Generic;

namespace WebTruyen.Models
{
    public partial class Favorite
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }

        public virtual Book Book { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
