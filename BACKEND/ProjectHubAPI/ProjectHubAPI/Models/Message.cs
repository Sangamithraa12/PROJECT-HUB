using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectHubAPI.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
    }
}
