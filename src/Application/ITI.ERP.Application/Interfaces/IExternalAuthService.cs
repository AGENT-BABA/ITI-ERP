using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;

namespace ITI.ERP.Application.Interfaces;

public interface IExternalAuthService
{
    Task<Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>> ExternalLoginAsync(string provider, string externalUserId, string? email, string? firstName, string? lastName, CancellationToken ct);
}
