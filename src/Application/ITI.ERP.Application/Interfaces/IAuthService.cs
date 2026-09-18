using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;

namespace ITI.ERP.Application.Interfaces;

public interface IAuthService
{
    Task<Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<Result<(TokenResponse Response, RefreshTokenResult RefreshToken)>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct);
    Task<Result> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct);
    Task<Result> LogoutAsync(Guid userId, CancellationToken ct);
    Task<Result> SetupSuperAdminAsync(SetupSuperAdminRequest request, CancellationToken ct);
    Task<Result<(TokenResponse Response, RefreshTokenResult RefreshToken)>> SwitchSessionAsync(Guid sessionId, CancellationToken ct);
}
