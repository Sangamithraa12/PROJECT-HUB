using System;

namespace ProjectHubAPI.DTOs
{
    public class AssignCourseDto
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int AssignedById { get; set; }
        public DateTime DueDate { get; set; }
    }
}
 
