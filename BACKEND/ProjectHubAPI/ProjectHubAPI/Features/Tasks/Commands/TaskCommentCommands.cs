using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Data;
using ProjectHubAPI.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Tasks.Commands
{
    public class AddTaskCommentHandler : IRequestHandler<AddTaskCommentCommand, ServiceResponse<CommentDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AddTaskCommentHandler(AppDbContext context, IMapper mapper)
        {
            _context = context; _mapper = mapper;
        }

        public async Task<ServiceResponse<CommentDto>> Handle(AddTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<CommentDto>.Fail("Task not found");

            var comment = new Comment { TaskId = request.TaskId, UserId = request.UserId, Content = request.Content, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(cancellationToken);

            if (task.AssignedTo != request.UserId)
            {
                var userName = await _context.Users.Where(u => u.Id == request.UserId).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);
                _context.Notifications.Add(new Notification { UserId = task.AssignedTo, Title = "New Message", Message = $"{userName} commented on your task: {task.Title}", Type = "Chat", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow });
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user?.Role?.Name != "Manager")
            {
                var managers = await _context.Users.Where(u => u.Role!.Name == "Manager").ToListAsync(cancellationToken);
                foreach (var manager in managers)
                {
                    if (manager.Id != task.AssignedTo && manager.Id != request.UserId)
                    {
                        _context.Notifications.Add(new Notification { UserId = manager.Id, Title = "Team Discussion", Message = $"{user?.Name} commented on: {task.Title}", Type = "Chat", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow });
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<CommentDto>.Ok(_mapper.Map<CommentDto>(comment));
        }
    }

    public class AddTaskCommentWithFileHandler : IRequestHandler<AddTaskCommentWithFileCommand, ServiceResponse<CommentDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public AddTaskCommentWithFileHandler(AppDbContext context, IMapper mapper, IWebHostEnvironment env)
        {
            _context = context; _mapper = mapper; _env = env;
        }

        public async Task<ServiceResponse<CommentDto>> Handle(AddTaskCommentWithFileCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<CommentDto>.Fail("Task not found");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await request.FileStream.CopyToAsync(destinationStream, cancellationToken);
            }

            var fileUrl = $"/uploads/chat/{uniqueFileName}";
            var fileType = "file";
            var ext = Path.GetExtension(request.FileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp") fileType = "image";

            var comment = new Comment { TaskId = request.TaskId, UserId = request.UserId, Content = request.Content, FileUrl = fileUrl, FileType = fileType, CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(cancellationToken);

            if (task.AssignedTo != request.UserId)
            {
                var userName = await _context.Users.Where(u => u.Id == request.UserId).Select(u => u.Name).FirstOrDefaultAsync(cancellationToken);
                _context.Notifications.Add(new Notification { UserId = task.AssignedTo, Title = "New Media", Message = $"{userName} shared a {fileType} in {task.Title}", Type = "Chat", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync(cancellationToken);
            }

            var result = _mapper.Map<CommentDto>(comment);
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user != null) result.UserName = user.Name;

            return ServiceResponse<CommentDto>.Ok(result);
        }
    }

    public class UpdateTaskCommentHandler : IRequestHandler<UpdateTaskCommentCommand, ServiceResponse<CommentDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UpdateTaskCommentHandler(AppDbContext context, IMapper mapper)
        {
            _context = context; _mapper = mapper;
        }

        public async Task<ServiceResponse<CommentDto>> Handle(UpdateTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);
            if (comment == null) return ServiceResponse<CommentDto>.Fail("Comment not found");

            comment.Content = request.Content;
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<CommentDto>.Ok(_mapper.Map<CommentDto>(comment));
        }
    }

    public class DeleteTaskCommentHandler : IRequestHandler<DeleteTaskCommentCommand, ServiceResponse<bool>>
    {
        private readonly AppDbContext _context;

        public DeleteTaskCommentHandler(AppDbContext context) { _context = context; }

        public async Task<ServiceResponse<bool>> Handle(DeleteTaskCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.Comments.FindAsync(new object[] { request.CommentId }, cancellationToken);
            if (comment == null) return ServiceResponse<bool>.Fail("Comment not found");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResponse<bool>.Ok(true);
        }
    }
}
 
