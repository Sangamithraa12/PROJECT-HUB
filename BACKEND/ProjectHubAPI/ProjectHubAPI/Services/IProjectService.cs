using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace ProjectHubAPI.Services
{
    public interface IProjectService
    {
        Task<ServiceResponse<IEnumerable<ProjectDto>>> GetAllProjectsAsync();
        Task<ServiceResponse<ProjectDto>> GetProjectByIdAsync(int id);
        Task<ServiceResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto projectDto);
        Task<ServiceResponse<ProjectDto>> UpdateProjectAsync(int id, CreateProjectDto projectDto);
        Task<ServiceResponse<bool>> DeleteProjectAsync(int id);
        Task<ServiceResponse<string>> UploadProjectFileAsync(int id, string fileName, Stream stream);
        Task<ServiceResponse<string>> UploadProjectFolderAsync(int id, List<ProjectFileData> files);
    }
}
