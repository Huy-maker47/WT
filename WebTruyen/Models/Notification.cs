using System;
using System.Collections.Generic;

namespace WebTruyen.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public string? Link { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
