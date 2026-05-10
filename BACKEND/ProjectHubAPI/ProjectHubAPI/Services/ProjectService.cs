using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Models;
using ProjectHubAPI.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;

namespace ProjectHubAPI.Services
{
    public class ProjectFileData
    {
        public string FileName { get; set; } = string.Empty;
        public Stream Stream { get; set; } = Stream.Null;
    }

    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public ProjectService(AppDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IMapper mapper)
        {
            _context = context;
            _env = env;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<ProjectDto>>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects.AsNoTracking().ToListAsync();
            var data = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            return ServiceResponse<IEnumerable<ProjectDto>>.Ok(data);
        }

        public async Task<ServiceResponse<ProjectDto>> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    ProjectId = t.ProjectId,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = _context.Users.Where(u => u.Id == t.AssignedTo).Select(u => u.Name).FirstOrDefault(),
                    ProofUrl = t.ProofUrl
                }).ToListAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);
            projectDto.Tasks = tasks;

            return ServiceResponse<ProjectDto>.Ok(projectDto);
        }

        public async Task<ServiceResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = _mapper.Map<Project>(dto);
            project.StartDate = DateTime.UtcNow;
            project.EndDate = DateTime.UtcNow.AddMonths(1);
            project.FilesUrl = "";
            project.Status = "Active";

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<ProjectDto>(project);
            return ServiceResponse<ProjectDto>.Ok(result, "Project created successfully");
        }

        public async Task<ServiceResponse<ProjectDto>> UpdateProjectAsync(int id, CreateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return ServiceResponse<ProjectDto>.Fail("Project not found");

            _mapper.Map(dto, project);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<ProjectDto>(project);
            return ServiceResponse<ProjectDto>.Ok(result, "Project updated successfully");
        }

        public async Task<ServiceResponse<bool>> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return ServiceResponse<bool>.Fail("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Project deleted successfully");
        }

        public async Task<ServiceResponse<string>> UploadProjectFileAsync(int id, string fileName, Stream fileStream)
        {
            var project = await _context.Projects.FindAsync(id);
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
            await _context.SaveChangesAsync();
            return ServiceResponse<string>.Ok(fileUrl, "File uploaded successfully");
        }

        public async Task<ServiceResponse<string>> UploadProjectFolderAsync(int id, List<ProjectFileData> files)
        {
            var project = await _context.Projects.FindAsync(id);
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
            await _context.SaveChangesAsync();
            return ServiceResponse<string>.Ok(folderUrl, "Folder uploaded successfully");
        }
    }
}
