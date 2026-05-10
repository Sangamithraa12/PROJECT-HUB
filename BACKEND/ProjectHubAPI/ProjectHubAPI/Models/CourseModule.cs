using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHubAPI.Models
{
    public class CourseModule
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; } 

        public int OrderIndex { get; set; }
    }
}
