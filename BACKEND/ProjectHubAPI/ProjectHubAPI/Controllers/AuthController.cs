using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Common.Responses;
using System.Threading.Tasks;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            return HandleResponse(await _authService.LoginAsync(dto));
        }
    }
}

 
