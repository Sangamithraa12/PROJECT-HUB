namespace ProjectHubAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int TaskId { get; set; }
        public TaskItem Task { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string? FileUrl { get; set; }
        public string? FileType { get; set; } // e.g., "image", "document"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
 
