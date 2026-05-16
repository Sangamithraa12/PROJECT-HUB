using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.Common.Responses;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ServiceResponse<T> response)
        {
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}

 
