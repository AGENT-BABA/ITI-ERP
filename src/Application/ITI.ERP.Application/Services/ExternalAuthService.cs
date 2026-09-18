using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;
using ITI.ERP.Application.DTOs.User;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public ExternalAuthService(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>> ExternalLoginAsync(
        string provider, string externalUserId, string? email, string? firstName, string? lastName, CancellationToken ct)
    {
        var existingLogin = await _context.ExternalLogins
            .Include(el => el.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(el => el.Provider == provider && el.ExternalUserId == externalUserId, ct);

        if (existingLogin is not null)
        {
            var user = existingLogin.User;
            if (user.IsDeleted || !user.IsActive)
                return Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>.Failure("Account is not available.");

            return await GenerateTokensForUser(user, ct);
        }

        User? linkedUser = null;

        if (!string.IsNullOrEmpty(email))
        {
            linkedUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
        }

        if (linkedUser is null)
        {
            linkedUser = new User
            {
                Id = Guid.NewGuid(),
                Username = email ?? $"{provider}_{externalUserId}",
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N")),
                FirstName = firstName ?? email?.Split('@')[0] ?? "User",
                LastName = lastName,
                IsActive = true,
                InstituteId = null
            };

            _context.Users.Add(linkedUser);
        }

        var externalLogin = new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = linkedUser.Id,
            Provider = provider,
            ExternalUserId = externalUserId,
            Email = email
        };
        _context.ExternalLogins.Add(externalLogin);

        await _context.SaveChangesAsync(ct);

        return await GenerateTokensForUser(linkedUser, ct);
    }

    private async Task<Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>> GenerateTokensForUser(
        User user, CancellationToken ct)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user, roles, permissions, user.InstituteId, null, null, null);

        var refreshTokenHash = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshTokenHash, 12),
            JwtId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtTokenService.GetAccessTokenExpiryInMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                TradeId = null
            }
        };

        return Result<(LoginResponse Response, RefreshTokenResult RefreshToken)>.Success(
            (response, new RefreshTokenResult
            {
                RawToken = refreshTokenHash,
                ExpiresAt = refreshToken.ExpiresAt
            }));
    }
}
