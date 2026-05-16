using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHubAPI.Features.Tasks.Queries
{
    // ─── Query Definitions ───────────────────────────────────────────────────────
    public record GetAllTasksQuery() : IRequest<ServiceResponse<IEnumerable<TaskDto>>>;
    public record GetTaskByIdQuery(int Id) : IRequest<ServiceResponse<TaskDto>>;

    // ─── Handlers ────────────────────────────────────────────────────────────────
    public class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, ServiceResponse<IEnumerable<TaskDto>>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GetAllTasksHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<TaskDto>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .ToListAsync(cancellationToken);

            var data = _mapper.Map<IEnumerable<TaskDto>>(tasks);
            return ServiceResponse<IEnumerable<TaskDto>>.Ok(data);
        }
    }

    public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, ServiceResponse<TaskDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GetTaskByIdHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (task == null) return ServiceResponse<TaskDto>.Fail("Task not found");

            var data = _mapper.Map<TaskDto>(task);
            return ServiceResponse<TaskDto>.Ok(data);
        }
    }
}
 
