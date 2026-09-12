using Asp.Versioning;
using ITI.ERP.Application.DTOs.Auth;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ITI.ERP.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeTokenAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return BadRequest("Invalid user token.");

            var result = await _authService.LogoutAsync(parsedUserId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("setup")]
        [AllowAnonymous]
        [EnableRateLimiting("setup")]
        public async Task<IActionResult> SetupSuperAdmin([FromBody] SetupSuperAdminRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.SetupSuperAdminAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("switch-session/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> SwitchSession(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = await _authService.SwitchSessionAsync(sessionId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.ForgotPasswordAsync(request.Email, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("verify-reset-token")]
        [AllowAnonymous]
        [EnableRateLimiting("verifyToken")]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.VerifyResetTokenAsync(request.Token, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.ResetPasswordWithTokenAsync(request.Token, request.NewPassword, cancellationToken);
            return HandleResult(result);
        }
    }
}
