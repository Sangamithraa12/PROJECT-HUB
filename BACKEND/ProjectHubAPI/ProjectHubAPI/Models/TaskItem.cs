using System;
using System.Collections.Generic;

namespace ProjectHubAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Status { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public int AssignedTo { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("AssignedTo")]
        public User AssignedUser { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public string? ProofUrl { get; set; }
        
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

