using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Features.Projects.Commands;
using ProjectHubAPI.Features.Projects.Queries;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHubAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IProjectService _projectService;

        public ProjectController(IMediator mediator, IProjectService projectService)
        {
            _mediator = mediator;
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects() =>
            HandleResponse(await _mediator.Send(new GetAllProjectsQuery()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id) =>
            HandleResponse(await _mediator.Send(new GetProjectByIdQuery(id)));

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            var result = await _mediator.Send(new CreateProjectCommand(dto));
            return result.Success
                ? CreatedAtAction(nameof(GetProject), new { id = result.Data?.Id }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProject(int id, CreateProjectDto dto) =>
            HandleResponse(await _mediator.Send(new UpdateProjectCommand(id, dto)));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteProject(int id) =>
            HandleResponse(await _mediator.Send(new DeleteProjectCommand(id)));

        [HttpPost("{id}/upload")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ServiceResponse<string>.Fail("File is empty"));

            using var stream = file.OpenReadStream();
            return HandleResponse(await _projectService.UploadProjectFileAsync(id, file.FileName, stream));
        }

        [HttpPost("{id}/upload-folder")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadFolder(int id, List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(ServiceResponse<string>.Fail("No files uploaded"));

            var fileData = files.Select(f => new ProjectFileData
            {
                FileName = f.FileName,
                Stream = f.OpenReadStream()
            }).ToList();

            return HandleResponse(await _projectService.UploadProjectFolderAsync(id, fileData));
        }
    }
}
