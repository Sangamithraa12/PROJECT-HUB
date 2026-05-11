using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Services;
using ProjectHubAPI.Models.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MediatR;
using ProjectHubAPI.Features.Projects.Queries;
using System.Linq;

namespace ProjectHubAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;
        private readonly IMediator _mediator;

        public ProjectController(IProjectService projectService, IMediator mediator)
        {
            _projectService = projectService;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects() => 
            HandleResponse(await _projectService.GetAllProjectsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id) => 
            HandleResponse(await _mediator.Send(new GetProjectByIdQuery(id)));

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProject(CreateProjectDto projectDto)
        {
            var result = await _projectService.CreateProjectAsync(projectDto);
            return result.Success 
                ? CreatedAtAction(nameof(GetProject), new { id = result.Data?.Id }, result) 
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProject(int id, CreateProjectDto projectDto) => 
            HandleResponse(await _projectService.UpdateProjectAsync(id, projectDto));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteProject(int id) => 
            HandleResponse(await _projectService.DeleteProjectAsync(id));

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

            var fileData = files.Select(file => new ProjectFileData 
            { 
                FileName = file.FileName, 
                Stream = file.OpenReadStream() 
            }).ToList();

            return HandleResponse(await _projectService.UploadProjectFolderAsync(id, fileData));
        }
    }
}
