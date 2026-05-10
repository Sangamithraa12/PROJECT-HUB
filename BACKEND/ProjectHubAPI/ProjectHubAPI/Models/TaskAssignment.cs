namespace ProjectHubAPI.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskItem Task { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

