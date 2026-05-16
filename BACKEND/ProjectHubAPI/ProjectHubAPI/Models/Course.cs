using System.ComponentModel.DataAnnotations;

namespace ProjectHubAPI.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? ThumbnailUrl { get; set; }

        public string? Duration { get; set; }

        public string? Category { get; set; }
        
        public string? VideoUrl { get; set; }
        public string? ResourceUrl { get; set; }
        public string? QuizData { get; set; } 
        public string? TargetRole { get; set; } = "All"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
 
