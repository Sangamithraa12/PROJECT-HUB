using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Models;
using ProjectHubAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using ProjectHubAPI.Hubs;

namespace ProjectHubAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public TaskService(AppDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return null;

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
        {
            var task = _mapper.Map<TaskItem>(dto);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            if (task.AssignedTo > 0)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedTo,
                    Title = "Task Assigned",
                    Message = $"A new task has been assigned to you: {task.Title}",
                    Type = "Task",
                    RelatedId = task.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                var assignment = new TaskAssignment { TaskId = task.Id, UserId = task.AssignedTo };
                _context.TaskAssignments.Add(assignment);

                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group(task.AssignedTo.ToString()).SendAsync("ReceiveNotification", notification.Message);
            }

            return (await GetTaskByIdAsync(task.Id))!;
        }

        public async Task<TaskDto?> UpdateTaskAsync(int id, CreateTaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            int originalAssignedTo = task.AssignedTo;

            _mapper.Map(dto, task);
            await _context.SaveChangesAsync();

            if (dto.AssignedTo != originalAssignedTo && dto.AssignedTo > 0)
            {
                var notification = new Notification
                {
                    UserId = dto.AssignedTo,
                    Title = "Task Assigned",
                    Message = $"A task has been assigned to you: {task.Title}",
                    Type = "Task",
                    RelatedId = task.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                var assignment = new TaskAssignment { TaskId = task.Id, UserId = dto.AssignedTo };
                _context.TaskAssignments.Add(assignment);

                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group(dto.AssignedTo.ToString()).SendAsync("ReceiveNotification", notification.Message);
            }

            return await GetTaskByIdAsync(id);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTaskAsync(int taskId, int userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            var user = await _context.Users.FindAsync(userId);

            if (task == null || user == null) return false;

            var assignment = new TaskAssignment { TaskId = taskId, UserId = userId };
            _context.TaskAssignments.Add(assignment);
            task.AssignedTo = userId;

            var notification = new Notification
            {
                UserId = userId,
                Title = "Task Assigned",
                Message = $"A new task has been assigned to you: {task.Title}",
                Type = "Task",
                RelatedId = taskId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notification.Message);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int taskId, string status, int currentUserId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            task.Status = status;

            var notificationsToSend = new List<Notification>();

            if (task.AssignedTo != currentUserId)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedTo,
                    Title = "Task Status Updated",
                    Message = $"The status of your task '{task.Title}' has been changed to: {status}",
                    Type = "Task",
                    RelatedId = taskId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                notificationsToSend.Add(notification);
            }

            var managers = await _context.Users.Where(u => u.Role!.Name == "Manager").ToListAsync();
            foreach (var manager in managers)
            {
                if (manager.Id != currentUserId && manager.Id != task.AssignedTo)
                {
                    var notification = new Notification
                    {
                        UserId = manager.Id,
                        Title = "Team Status Update",
                        Message = $"Task '{task.Title}' updated to: {status}",
                        Type = "Task",
                        RelatedId = taskId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                    notificationsToSend.Add(notification);
                }
            }

            await _context.SaveChangesAsync();

            foreach(var n in notificationsToSend)
            {
                await _hubContext.Clients.Group(n.UserId.ToString()).SendAsync("ReceiveNotification", n.Message);
            }

            return true;
        }

        public async Task<bool> SubmitProofAsync(int taskId, string proofUrl)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            task.ProofUrl = proofUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> UploadProofAsync(int taskId, string fileName, Stream fileStream)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "tasks");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            var fileUrl = $"/uploads/tasks/{uniqueFileName}";
            task.ProofUrl = fileUrl;
            await _context.SaveChangesAsync();

            return fileUrl;
        }

        public async Task<string?> UploadFolderAsync(int taskId, List<(string fileName, Stream stream)> files)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var folderName = $"{Guid.NewGuid()}_proof";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "tasks", folderName);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                var filePath = Path.Combine(uploadDir, file.fileName);
                var subDir = Path.GetDirectoryName(filePath);
                if (subDir != null && !Directory.Exists(subDir)) Directory.CreateDirectory(subDir);

                using (var destinationStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.stream.CopyToAsync(destinationStream);
                }
            }

            var folderUrl = $"/uploads/tasks/{folderName}"; 
            task.ProofUrl = folderUrl;
            await _context.SaveChangesAsync();

            return folderUrl;
        }

        public async Task<CommentDto?> AddCommentAsync(int taskId, int userId, string content)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if (task.AssignedTo != userId)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedTo,
                    Title = "New Message",
                    Message = $"{await _context.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefaultAsync()} commented on your task: {task.Title}",
                    Type = "Chat",
                    RelatedId = taskId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Role?.Name != "Manager")
            {
                var managers = await _context.Users.Where(u => u.Role!.Name == "Manager").ToListAsync();
                foreach (var manager in managers)
                {
                    if (manager.Id != task.AssignedTo && manager.Id != userId)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = manager.Id,
                            Title = "Team Discussion",
                            Message = $"{user?.Name} commented on: {task.Title}",
                            Type = "Chat",
                            RelatedId = taskId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<CommentDto?> AddCommentWithFileAsync(int taskId, int userId, string content, string fileName, Stream fileStream)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            var fileUrl = $"/uploads/chat/{uniqueFileName}";
            var fileType = "file";
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp")
            {
                fileType = "image";
            }

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Content = content,
                FileUrl = fileUrl,
                FileType = fileType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

           
            if (task.AssignedTo != userId)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = task.AssignedTo,
                    Title = "New Media",
                    Message = $"{await _context.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefaultAsync()} shared a {fileType} in {task.Title}",
                    Type = "Chat",
                    RelatedId = taskId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var result = _mapper.Map<CommentDto>(comment);
            var user = await _context.Users.FindAsync(userId);
            if (user != null) result.UserName = user.Name;

            return result;
        }

        public async Task<CommentDto?> UpdateCommentAsync(int commentId, string content)
        {
            var comment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return null;

            comment.Content = content;
            await _context.SaveChangesAsync();

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
