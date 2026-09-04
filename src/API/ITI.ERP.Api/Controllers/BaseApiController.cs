using ITI.ERP.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);
            if (result.Errors?.Count > 0)
                return BadRequest(new { error = result.Errors });
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("access denied", StringComparison.OrdinalIgnoreCase) == true)
                return StatusCode(403, new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok();
            if (result.Errors?.Count > 0)
                return BadRequest(new { error = result.Errors });
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                return NotFound(new { error = result.Error });
            if (result.Error?.Contains("access denied", StringComparison.OrdinalIgnoreCase) == true)
                return StatusCode(403, new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
    }
}
