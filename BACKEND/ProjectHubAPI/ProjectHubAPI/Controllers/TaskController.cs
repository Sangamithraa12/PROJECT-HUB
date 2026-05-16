using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Features.Tasks.Commands;
using ProjectHubAPI.Features.Tasks.Queries;
using ProjectHubAPI.Common.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : BaseController
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks() =>
            HandleResponse(await _mediator.Send(new GetAllTasksQuery()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id) =>
            HandleResponse(await _mediator.Send(new GetTaskByIdQuery(id)));

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateTask(CreateTaskDto taskDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _mediator.Send(new CreateTaskCommand(taskDto));
            return result.Success
                ? CreatedAtAction(nameof(GetTask), new { id = result.Data?.Id }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateTask(int id, CreateTaskDto taskDto) =>
            HandleResponse(await _mediator.Send(new UpdateTaskCommand(id, taskDto)));

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteTask(int id) =>
            HandleResponse(await _mediator.Send(new DeleteTaskCommand(id)));

        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] int userId) =>
            HandleResponse(await _mediator.Send(new AssignTaskCommand(id, userId)));

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized("User ID not found in token");
            return HandleResponse(await _mediator.Send(new UpdateTaskStatusCommand(id, status, userId)));
        }

        [HttpPost("{id}/proof")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> SubmitProof(int id, [FromBody] string proofUrl) =>
            HandleResponse(await _mediator.Send(new SubmitTaskProofCommand(id, proofUrl)));

        [HttpPost("{id}/upload")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadProof(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");
            using var stream = file.OpenReadStream();
            return HandleResponse(await _mediator.Send(new UploadTaskProofCommand(id, file.FileName, stream)));
        }

        [HttpPost("{id}/upload-folder")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadFolder(int id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest("No files uploaded");
            var fileData = files.Select(f => (f.FileName, (System.IO.Stream)f.OpenReadStream())).ToList();
            return HandleResponse(await _mediator.Send(new UploadTaskFolderCommand(id, fileData)));
        }

        [HttpPost("{id}/comment")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddComment(int id, [FromBody] string content)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized("User ID not found in token");
            return HandleResponse(await _mediator.Send(new AddTaskCommentCommand(id, userId, content)));
        }

        [HttpPut("comment/{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] string content) =>
            HandleResponse(await _mediator.Send(new UpdateTaskCommentCommand(id, content)));

        [HttpPost("{taskId}/comment/file")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddCommentWithFile(int taskId, [FromForm] string content, IFormFile file)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            using var stream = file.OpenReadStream();
            return HandleResponse(await _mediator.Send(new AddTaskCommentWithFileCommand(taskId, userId, content, file.FileName, stream)));
        }

        [HttpDelete("comment/{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> DeleteComment(int id) =>
            HandleResponse(await _mediator.Send(new DeleteTaskCommentCommand(id)));
    }
}
