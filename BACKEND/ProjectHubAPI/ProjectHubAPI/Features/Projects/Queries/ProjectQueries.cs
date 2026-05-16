using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Common.Responses;
using MapsterMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Projects.Queries
{

    public record GetAllProjectsQuery() : IRequest<ServiceResponse<IEnumerable<ProjectDto>>>;

    public record GetProjectByIdQuery(int Id) : IRequest<ServiceResponse<ProjectDto>>;

    public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, ServiceResponse<IEnumerable<ProjectDto>>>
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IMapper _mapper;

        public GetAllProjectsHandler(IProjectRepository projectRepo, IMapper mapper)
        {
            _projectRepo = projectRepo;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<ProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var projects = await _projectRepo.GetAllAsync();
            var data = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            return ServiceResponse<IEnumerable<ProjectDto>>.Ok(data);
        }
    }

    public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ServiceResponse<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IMapper _mapper;

        public GetProjectByIdHandler(IProjectRepository projectRepo, ITaskRepository taskRepo, IMapper mapper)
        {
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(request.Id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(request.Id);
            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var projectDto = _mapper.Map<ProjectDto>(project);
            projectDto.Tasks = taskDtos;

            return ServiceResponse<ProjectDto>.Ok(projectDto);
        }
    }
}
