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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Tasks.Commands
{
    public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, ServiceResponse<bool>>
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public UpdateTaskStatusHandler(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context; _hubContext = hubContext;
        }

        public async Task<ServiceResponse<bool>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return ServiceResponse<bool>.Fail("Task not found");

            task.Status = request.Status;
            var notificationsToSend = new List<Notification>();

            if (task.AssignedTo != request.CurrentUserId)
            {
                var notification = new Notification { UserId = task.AssignedTo, Title = "Task Status Updated", Message = $"The status of your task '{task.Title}' has been changed to: {request.Status}", Type = "Task", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow };
                _context.Notifications.Add(notification);
                notificationsToSend.Add(notification);
            }

            var managers = await _context.Users.Where(u => u.Role!.Name == "Manager").ToListAsync(cancellationToken);
            foreach (var manager in managers)
            {
                if (manager.Id != request.CurrentUserId && manager.Id != task.AssignedTo)
                {
                    var notification = new Notification { UserId = manager.Id, Title = "Team Status Update", Message = $"Task '{task.Title}' updated to: {request.Status}", Type = "Task", RelatedId = request.TaskId, CreatedAt = DateTime.UtcNow };
                    _context.Notifications.Add(notification);
                    notificationsToSend.Add(notification);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var n in notificationsToSend)
            {
                await _hubContext.Clients.Group(n.UserId.ToString()).SendAsync("ReceiveNotification", n.Message, cancellationToken);
            }

            await _hubContext.Clients.All.SendAsync("RefreshTasks", cancellationToken);
            return ServiceResponse<bool>.Ok(true);
        }
    }
}
