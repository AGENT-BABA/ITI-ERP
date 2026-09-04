using Asp.Versioning;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Application.DTOs.User;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion("1.0")]
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Users.View)]
        [ProducesResponseType(typeof(PaginatedList<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetUsersAsync(new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Users.View)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.GetUserByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.CreateUserAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateUserAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id}/toggle-status")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.ToggleUserStatusAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/send-password-reset")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPasswordReset(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.SendPasswordResetAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id}/unlock")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnlockUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.UnlockUserAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeleteUserAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("tradeheads/{instituteId}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        [ProducesResponseType(typeof(List<TradeHeadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTradeHeadsByInstitute(Guid instituteId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetTradeHeadsByInstituteAsync(instituteId, cancellationToken);
            return HandleResult(result);
        }
    }
}
