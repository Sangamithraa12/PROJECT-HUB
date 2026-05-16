using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Models;
using ProjectHubAPI.Common.Responses;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using ProjectHubAPI.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Projects.Commands
{
    // ─── Command Definitions ─────────────────────────────────────────────────────
    public record CreateProjectCommand(CreateProjectDto Dto) : IRequest<ServiceResponse<ProjectDto>>;
    public record UpdateProjectCommand(int Id, CreateProjectDto Dto) : IRequest<ServiceResponse<ProjectDto>>;
    public record DeleteProjectCommand(int Id) : IRequest<ServiceResponse<bool>>;

    // ─── Handlers ────────────────────────────────────────────────────────────────
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ServiceResponse<ProjectDto>>
    {
        private readonly IProjectRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public CreateProjectHandler(IProjectRepository repo, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _repo = repo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ServiceResponse<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = _mapper.Map<Project>(request.Dto);
            project.StartDate = DateTime.UtcNow;
            project.EndDate = DateTime.UtcNow.AddMonths(1);
            project.FilesUrl = "";
            project.Status = "Active";

            await _repo.AddAsync(project);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");

            return ServiceResponse<ProjectDto>.Ok(_mapper.Map<ProjectDto>(project), "Project created successfully");
        }
    }

    public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ServiceResponse<ProjectDto>>
    {
        private readonly IProjectRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public UpdateProjectHandler(IProjectRepository repo, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _repo = repo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ServiceResponse<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _repo.GetByIdAsync(request.Id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            _mapper.Map(request.Dto, project);
            await _repo.UpdateAsync(project);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");

            return ServiceResponse<ProjectDto>.Ok(_mapper.Map<ProjectDto>(project), "Project updated successfully");
        }
    }

    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, ServiceResponse<bool>>
    {
        private readonly IProjectRepository _repo;
        private readonly IHubContext<ChatHub> _hubContext;

        public DeleteProjectHandler(IProjectRepository repo, IHubContext<ChatHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        public async Task<ServiceResponse<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _repo.GetByIdAsync(request.Id);
            if (project == null) return ServiceResponse<bool>.Fail("Project not found");

            await _repo.DeleteAsync(project);
            await _repo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");

            return ServiceResponse<bool>.Ok(true, "Project deleted successfully");
        }
    }
}

 
