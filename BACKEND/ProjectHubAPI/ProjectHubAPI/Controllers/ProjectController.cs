using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Services;
using ProjectHubAPI.Models.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<ProjectDto>>>> GetProjects()
        {
            var result = await _projectService.GetAllProjectsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<ProjectDto>>> GetProject(int id)
        {
            var result = await _projectService.GetProjectByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ServiceResponse<ProjectDto>>> CreateProject(CreateProjectDto projectDto)
        {
            var result = await _projectService.CreateProjectAsync(projectDto);
            return CreatedAtAction(nameof(GetProject), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ServiceResponse<ProjectDto>>> UpdateProject(int id, CreateProjectDto projectDto)
        {
            var result = await _projectService.UpdateProjectAsync(id, projectDto);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteProject(int id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id}/upload")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<ActionResult<ServiceResponse<string>>> UploadFile(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) 
                return BadRequest(ServiceResponse<string>.Fail("File is empty"));

            using (var stream = file.OpenReadStream())
            {
                var result = await _projectService.UploadProjectFileAsync(id, file.FileName, stream);
                if (!result.Success) return NotFound(result);

                return Ok(result);
            }
        }

        [HttpPost("{id}/upload-folder")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<ActionResult<ServiceResponse<string>>> UploadFolder(int id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0) 
                return BadRequest(ServiceResponse<string>.Fail("No files uploaded"));

            var fileData = files.Select(file => new ProjectFileData 
            { 
                FileName = file.FileName, 
                Stream = file.OpenReadStream() 
            }).ToList();

            var result = await _projectService.UploadProjectFolderAsync(id, fileData);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }
    }
}
