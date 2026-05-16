using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHubAPI.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Enrolled";

        public int ProgressPercentage { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletionDate { get; set; }
        public int QuizScore { get; set; }

        public bool IsMandatory { get; set; } = false;
        public int? AssignedById { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
 
