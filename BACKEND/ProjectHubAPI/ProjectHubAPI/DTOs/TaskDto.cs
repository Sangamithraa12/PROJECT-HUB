using System;
using System.Collections.Generic;

namespace ProjectHubAPI.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ProofUrl { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
    }

    public class CreateTaskDto
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int ProjectId { get; set; }
        public int AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ProofUrl { get; set; }
    }
}

 
