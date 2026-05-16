using ProjectHubAPI.DTOs;
using ProjectHubAPI.Common.Responses;
using System.Threading.Tasks;

namespace ProjectHubAPI.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    }
}
