using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectHubAPI.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
        public DateTime? DueDate { get; set; }

        public decimal Budget { get; set; }

        public string? Status { get; set; }
        
        public string? FilesUrl { get; set; }
    }
}
