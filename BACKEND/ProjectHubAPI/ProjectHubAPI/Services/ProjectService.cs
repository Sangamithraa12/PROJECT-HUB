using ProjectHubAPI.DTOs;
using ProjectHubAPI.Models;
using ProjectHubAPI.Common.Responses;
using ProjectHubAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using ProjectHubAPI.Hubs;

namespace ProjectHubAPI.Services
{
    public class ProjectFileData
    {
        public string FileName { get; set; } = string.Empty;
        public Stream Stream { get; set; } = Stream.Null;
    }

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public ProjectService(
            IProjectRepository projectRepo, 
            ITaskRepository taskRepo,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, 
            IMapper mapper,
            IHubContext<ChatHub> hubContext)
        {
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _env = env;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ServiceResponse<IEnumerable<ProjectDto>>> GetAllProjectsAsync()
        {
            var projects = await _projectRepo.GetAllAsync();
            var data = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            return ServiceResponse<IEnumerable<ProjectDto>>.Ok(data);
        }

        public async Task<ServiceResponse<ProjectDto>> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(id);
            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var projectDto = _mapper.Map<ProjectDto>(project);
            projectDto.Tasks = taskDtos;

            return ServiceResponse<ProjectDto>.Ok(projectDto);
        }

        public async Task<ServiceResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = _mapper.Map<Project>(dto);
            project.StartDate = DateTime.UtcNow;
            project.EndDate = DateTime.UtcNow.AddMonths(1);
            project.FilesUrl = "";
            project.Status = "Active";

            await _projectRepo.AddAsync(project);
            await _projectRepo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");

            var result = _mapper.Map<ProjectDto>(project);
            return ServiceResponse<ProjectDto>.Ok(result, "Project created successfully");
        }

        public async Task<ServiceResponse<ProjectDto>> UpdateProjectAsync(int id, CreateProjectDto dto)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            _mapper.Map(dto, project);
            await _projectRepo.UpdateAsync(project);
            await _projectRepo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");

            var result = _mapper.Map<ProjectDto>(project);
            return ServiceResponse<ProjectDto>.Ok(result, "Project updated successfully");
        }

        public async Task<ServiceResponse<bool>> DeleteProjectAsync(int id)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return ServiceResponse<bool>.Fail("Project not found");

            await _projectRepo.DeleteAsync(project);
            await _projectRepo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("RefreshProjects");
            return ServiceResponse<bool>.Ok(true, "Project deleted successfully");
        }

        public async Task<ServiceResponse<string>> UploadProjectFileAsync(int id, string fileName, Stream fileStream)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return ServiceResponse<string>.Fail("Project not found");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "projects");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            var fileUrl = $"/uploads/projects/{uniqueFileName}";
            project.FilesUrl = fileUrl;
            await _projectRepo.UpdateAsync(project);
            await _projectRepo.SaveChangesAsync();
            return ServiceResponse<string>.Ok(fileUrl, "File uploaded successfully");
        }

        public async Task<ServiceResponse<string>> UploadProjectFolderAsync(int id, List<ProjectFileData> files)
        {
            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return ServiceResponse<string>.Fail("Project not found");

            var folderName = $"{Guid.NewGuid()}_files";
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "projects", folderName);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                var filePath = Path.Combine(uploadDir, file.FileName);
                var subDir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(subDir) && !Directory.Exists(subDir)) Directory.CreateDirectory(subDir);

                using (var destinationStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.Stream.CopyToAsync(destinationStream);
                }
            }

            var folderUrl = $"/uploads/projects/{folderName}"; 
            project.FilesUrl = folderUrl;
            await _projectRepo.UpdateAsync(project);
            await _projectRepo.SaveChangesAsync();
            return ServiceResponse<string>.Ok(folderUrl, "Folder uploaded successfully");
        }
    }
}

