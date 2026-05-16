using System.Collections.Generic;

namespace ProjectHubAPI.DTOs
{
    public class CourseModuleDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; } 
    }

    public class CreateCourseModuleDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int OrderIndex { get; set; }
    }

    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Duration { get; set; }
        public string Category { get; set; }
        public string VideoUrl { get; set; }
        public string ResourceUrl { get; set; }
        public string QuizData { get; set; }
        public List<CourseModuleDto> Modules { get; set; } = new List<CourseModuleDto>();
    }
}
 
