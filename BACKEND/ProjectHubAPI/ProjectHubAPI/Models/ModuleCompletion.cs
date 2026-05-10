using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHubAPI.Models
{
    public class ModuleCompletion
    {
        public int Id { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [ForeignKey("EnrollmentId")]
        public Enrollment Enrollment { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public CourseModule Module { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CompletedDate { get; set; } = DateTime.UtcNow;
    }
}
