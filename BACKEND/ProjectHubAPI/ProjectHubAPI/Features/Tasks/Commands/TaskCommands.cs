using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Data;
using ProjectHubAPI.Models;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Tasks.Commands
{
    // ─── Command Definitions ─────────────────────────────────────────────────────
    public record CreateTaskCommand(CreateTaskDto Dto) : IRequest<ServiceResponse<TaskDto>>;
    public record UpdateTaskCommand(int Id, CreateTaskDto Dto) : IRequest<ServiceResponse<TaskDto>>;
    public record DeleteTaskCommand(int Id) : IRequest<ServiceResponse<bool>>;
    public record AssignTaskCommand(int TaskId, int UserId) : IRequest<ServiceResponse<bool>>;
    
    public record UpdateTaskStatusCommand(int TaskId, string Status, int CurrentUserId) : IRequest<ServiceResponse<bool>>;
    public record SubmitTaskProofCommand(int TaskId, string ProofUrl) : IRequest<ServiceResponse<bool>>;
    public record UploadTaskProofCommand(int TaskId, string FileName, Stream FileStream) : IRequest<ServiceResponse<string>>;
    public record UploadTaskFolderCommand(int TaskId, List<(string fileName, Stream stream)> Files) : IRequest<ServiceResponse<string>>;
    
    public record AddTaskCommentCommand(int TaskId, int UserId, string Content) : IRequest<ServiceResponse<CommentDto>>;
    public record AddTaskCommentWithFileCommand(int TaskId, int UserId, string Content, string FileName, Stream FileStream) : IRequest<ServiceResponse<CommentDto>>;
    public record UpdateTaskCommentCommand(int CommentId, string Content) : IRequest<ServiceResponse<CommentDto>>;
    public record DeleteTaskCommentCommand(int CommentId) : IRequest<ServiceResponse<bool>>;

    // ─── Handlers ────────────────────────────────────────────────────────────────
    public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, ServiceResponse<TaskDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public CreateTaskHandler(AppDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context; _mapper = mapper; _hubContext = hubContext;
        }

        public async Task<ServiceResponse<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = _mapper.Map<TaskItem>(request.Dto);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);

            if (task.AssignedTo > 0)
            {
                var notification = new Notification { UserId = task.AssignedTo, Title = "Task Assigned", Message = $"A new task has been assigned to you: {task.Title}", Type = "Task", RelatedId = task.Id, CreatedAt = DateTime.UtcNow };
                _context.Notifications.Add(notification);
                _context.TaskAssignments.Add(new TaskAssignment { TaskId = task.Id, UserId = task.AssignedTo });
                await _context.SaveChangesAsync(cancellationToken);
                await _hubContext.Clients.Group(task.AssignedTo.ToString()).SendAsync("ReceiveNotification", notification.Message, cancellationToken);
            }

            await _hubContext.Clients.All.SendAsync("RefreshTasks", cancellationToken);

            var createdTask = await _context.Tasks.Include(t => t.Project).Include(t => t.AssignedUser).Include(t => t.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken);
            return ServiceResponse<TaskDto>.Ok(_mapper.Map<TaskDto>(createdTask));
        }
    }

    public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, ServiceResponse<TaskDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public UpdateTaskHandler(AppDbContext context, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _context = context; _mapper = mapper; _hubContext = hubContext;
        }

        public async Task<ServiceResponse<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);
            if (task == null) return ServiceResponse<TaskDto>.Fail("Task not found");

            int originalAssignedTo = task.AssignedTo;
            _mapper.Map(request.Dto, task);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.Dto.AssignedTo != originalAssignedTo && request.Dto.AssignedTo > 0)
            {
                var notification = new Notification { UserId = request.Dto.AssignedTo, Title = "Task Assigned", Message = $"A task has been assigned to you: {task.Title}", Type = "Task", RelatedId = task.Id, CreatedAt = DateTime.UtcNow };
                _context.Notifications.Add(notification);
                _context.TaskAssignments.Add(new TaskAssignment { TaskId = task.Id, UserId = request.Dto.AssignedTo });
                await _context.SaveChangesAsync(cancellationToken);
                await _hubContext.Clients.Group(request.Dto.AssignedTo.ToString()).SendAsync("ReceiveNotification", notification.Message, cancellationToken);
            }

            await _hubContext.Clients.All.SendAsync("RefreshTasks", cancellationToken);
            var updatedTask = await _context.Tasks.Include(t => t.Project).Include(t => t.AssignedUser).Include(t => t.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken);
            return ServiceResponse<TaskDto>.Ok(_mapper.Map<TaskDto>(updatedTask));
        }
    }

    public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, ServiceResponse<bool>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public DeleteTaskHandler(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context; _hubContext = hubContext;
        }

        public async Task<ServiceResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);
            if (task == null) return ServiceResponse<bool>.Fail("Task not found");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
            await _hubContext.Clients.All.SendAsync("RefreshTasks", cancellationToken);
            return ServiceResponse<bool>.Ok(true);
        }
    }

    public class AssignTaskHandler : IRequestHandler<AssignTaskCommand, ServiceResponse<bool>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public AssignTaskHandler(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context; _hubContext = hubContext;
        }

        public async Task<ServiceResponse<bool>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (task == null || user == null) return ServiceResponse<bool>.Fail("Task or User not found");

            _context.TaskAssignments.Add(new TaskAssignment { TaskId = request.TaskId, UserId = request.UserId });
            task.AssignedTo = request.UserId;

            var notification = new Notification { UserId = request.UserId, Title = "Task Assigned", Message = $"A new task has been assigned to you: {task.Title}", Type = "Task", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);
            await _hubContext.Clients.Group(request.UserId.ToString()).SendAsync("ReceiveNotification", notification.Message, cancellationToken);
            return ServiceResponse<bool>.Ok(true);
        }
    }
}
 
