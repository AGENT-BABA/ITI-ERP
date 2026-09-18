using Asp.Versioning;
using System.Security.Claims;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
        private readonly IExternalAuthService _externalAuthService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        private const string RefreshTokenCookieName = "refresh_token";

        public AuthController(
            IAuthService authService,
            IExternalAuthService externalAuthService,
            IUserService userService,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _authService = authService;
            _externalAuthService = externalAuthService;
            _userService = userService;
            _env = env;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (!result.IsSuccess)
                return HandleResult(Result<LoginResponse>.Failure(result.Error ?? "Login failed."));

            var (response, refreshToken) = result.Value!;
            SetRefreshTokenCookie(refreshToken.RawToken, refreshToken.ExpiresAt);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var cookieToken = Request.Cookies[RefreshTokenCookieName];
            var tokenToUse = !string.IsNullOrEmpty(cookieToken) ? cookieToken : request.RefreshToken;

            if (string.IsNullOrEmpty(tokenToUse))
            {
                ClearRefreshTokenCookie();
                return Unauthorized(new { error = "Refresh token not found." });
            }

            var serviceRequest = new RefreshTokenRequest
            {
                RefreshToken = tokenToUse,
                AcademicSessionId = request.AcademicSessionId
            };

            var result = await _authService.RefreshTokenAsync(serviceRequest, cancellationToken);
            if (!result.IsSuccess)
            {
                ClearRefreshTokenCookie();
                return HandleResult(Result<TokenResponse>.Failure(result.Error ?? "Refresh failed."));
            }

            var (response, refreshToken) = result.Value!;
            SetRefreshTokenCookie(refreshToken.RawToken, refreshToken.ExpiresAt);
            return Ok(response);
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
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return BadRequest("Invalid user token.");

            var result = await _authService.LogoutAsync(parsedUserId, cancellationToken);
            ClearRefreshTokenCookie();
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
            if (!result.IsSuccess)
                return HandleResult(Result<TokenResponse>.Failure(result.Error ?? "Session switch failed."));

            var (response, refreshToken) = result.Value!;
            SetRefreshTokenCookie(refreshToken.RawToken, refreshToken.ExpiresAt);
            return Ok(response);
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

        [HttpGet("external/google")]
        [AllowAnonymous]
        public IActionResult ChallengeGoogle(string? returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl }),
                Items =
                {
                    { "LoginProvider", GoogleDefaults.AuthenticationScheme }
                }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("external/google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? returnUrl, CancellationToken cancellationToken)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
                return Redirect($"{GetFrontendUrl()}/login?error=google_auth_failed");

            var claims = authenticateResult.Principal;
            var externalUserId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var email = claims.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var firstName = claims.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = claims.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

            var result = await _externalAuthService.ExternalLoginAsync(
                "Google", externalUserId, email, firstName, lastName, cancellationToken);

            if (!result.IsSuccess)
                return Redirect($"{GetFrontendUrl()}/login?error={Uri.EscapeDataString(result.Error ?? "Login failed")}");

            var (response, refreshToken) = result.Value!;
            SetRefreshTokenCookie(refreshToken.RawToken, refreshToken.ExpiresAt);

            var frontendUrl = GetFrontendUrl();
            var callbackUrl = $"{frontendUrl}/auth/callback?accessToken={Uri.EscapeDataString(response.AccessToken)}&expiresAt={Uri.EscapeDataString(response.ExpiresAt.ToString("o"))}";

            return Redirect(callbackUrl);
        }

        private string GetFrontendUrl()
        {
            return _configuration["Email:FrontendBaseUrl"] ?? "http://localhost:5173";
        }

        private void SetRefreshTokenCookie(string rawToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                Path = "/"
            };
            Response.Cookies.Append(RefreshTokenCookieName, rawToken, cookieOptions);
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Lax
            });
        }
    }
}
