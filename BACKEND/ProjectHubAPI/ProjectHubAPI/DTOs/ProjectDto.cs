using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectHubAPI.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Range(0, 1000000000)]
        public decimal Budget { get; set; }
        
        [Required]
        public string Status { get; set; }
        
        public string FilesUrl { get; set; }
        
        public List<TaskDto> Tasks { get; set; } = new List<TaskDto>();
    }

    public class CreateProjectDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Range(0, 1000000000)]
        public decimal Budget { get; set; }
        
        public string? Status { get; set; }
        
        public string? FilesUrl { get; set; }
    }
}
