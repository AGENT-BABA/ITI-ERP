using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Auth;
using ITI.ERP.Application.DTOs.User;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        const string SuperAdminGR = "GR0000";
        User? user;

        if (string.Equals(request.GRNumber, SuperAdminGR, StringComparison.OrdinalIgnoreCase))
        {
            user = await _context.Users.IgnoreQueryFilters()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Batch)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username
                    && !u.IsDeleted
                    && u.UserRoles.Any(ur => ur.Role.Name == RoleConstants.Admin), ct);
        }
        else
        {
            var institute = await _context.Institutes
                .FirstOrDefaultAsync(i => i.GRNumber == request.GRNumber && i.IsActive, ct);

            if (institute is null)
                return Result<LoginResponse>.Failure("Invalid GR Number.");

            user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Batch)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.InstituteId == institute.Id && !u.IsDeleted, ct);
        }

        if (user is null)
            return Result<LoginResponse>.Failure("Invalid username or password.");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is deactivated.");

        if (user.IsLocked)
        {
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
                return Result<LoginResponse>.Failure("Account is locked. Please try again later.");

            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }
            await _context.SaveChangesAsync(ct);
            return Result<LoginResponse>.Failure("Invalid username or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var (instituteId, tradeId, batchId) = ResolveScope(user, roles);

        var activeSession = instituteId.HasValue
            ? await _context.AcademicSessions
                .FirstOrDefaultAsync(s => s.InstituteId == instituteId.Value && s.IsActive && !s.IsDeleted, ct)
            : null;
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions, instituteId, activeSession?.Id, tradeId, batchId);
        var refreshTokenHash = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            JwtId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        await _context.RefreshTokens.AddAsync(refreshToken, ct);

        await _context.SaveChangesAsync(ct);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.TokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfileImagePath = user.ProfileImagePath,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                Roles = roles,
                TradeId = tradeId,
                LastLoginAt = user.LastLoginAt
            }
        });
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Batch)
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.TokenHash == request.RefreshToken && rt.IsActive, ct);

        if (refreshToken is null || refreshToken.ExpiresAt < DateTime.UtcNow)
            return Result<TokenResponse>.Failure("Invalid or expired refresh token.");

        refreshToken.IsActive = false;
        refreshToken.RevokedAt = DateTime.UtcNow;

        var user = refreshToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var (instituteId, tradeId, batchId) = ResolveScope(user, roles);

        // Preserve user's selected session if it's still valid; otherwise fall back to active
        Guid? sessionToUse = null;
        if (request.AcademicSessionId.HasValue)
        {
            var requestedSession = await _context.AcademicSessions
                .FirstOrDefaultAsync(s => s.Id == request.AcademicSessionId.Value && !s.IsDeleted, ct);
            if (requestedSession is not null &&
                (!instituteId.HasValue || requestedSession.InstituteId == instituteId.Value))
            {
                sessionToUse = requestedSession.Id;
            }
        }

        if (sessionToUse is null)
        {
            var activeSession = instituteId.HasValue
                ? await _context.AcademicSessions
                    .FirstOrDefaultAsync(s => s.InstituteId == instituteId.Value && s.IsActive && !s.IsDeleted, ct)
                : null;
            sessionToUse = activeSession?.Id;
        }

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions, instituteId, sessionToUse, tradeId, batchId);
        var newRefreshTokenHash = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            JwtId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        await _context.RefreshTokens.AddAsync(newRefreshToken, ct);

        await _context.SaveChangesAsync(ct);

        return Result<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.TokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }

    public async Task<Result> RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == request.RefreshToken && rt.IsActive, ct);

        if (token is null)
            return Result.Failure("Refresh token not found.");

        token.IsActive = false;
        token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken ct)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync(ct);

        foreach (var token in refreshTokens)
        {
            token.IsActive = false;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private static (Guid? instituteId, Guid? tradeId, Guid? batchId) ResolveScope(User user, List<string> roles)
    {
        if (roles.Contains(RoleConstants.Admin))
            return (null, null, null);

        if (roles.Contains(RoleConstants.TradeHead))
        {
            var tradeHeadRole = user.UserRoles
                .FirstOrDefault(ur => ur.Role.Name == RoleConstants.TradeHead);
            var instituteId = tradeHeadRole?.InstituteId ?? user.InstituteId;
            return (instituteId, tradeHeadRole?.TradeId, tradeHeadRole?.BatchId);
        }

        if (roles.Contains(RoleConstants.InstituteAdmin))
        {
            var instituteAdminRole = user.UserRoles
                .FirstOrDefault(ur => ur.Role.Name == RoleConstants.InstituteAdmin);
            return (instituteAdminRole?.InstituteId, null, null);
        }

        var fallbackRole = user.UserRoles.FirstOrDefault(ur => ur.InstituteId != null);
        return (fallbackRole?.InstituteId, fallbackRole?.TradeId, null);
    }

    public async Task<Result> SetupSuperAdminAsync(SetupSuperAdminRequest request, CancellationToken ct)
    {
        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.Admin, ct);

        if (adminRole is null)
            return Result.Failure("Admin role not found. Run database seed first.");

        var existingAdminUser = await _context.UserRoles
            .AnyAsync(ur => ur.RoleId == adminRole.Id, ct);

        if (existingAdminUser)
            return Result.Failure("A SuperAdmin user already exists. Setup cannot be repeated.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username && !u.IsDeleted, ct))
            return Result.Failure("Username already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            InstituteId = null,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email?.Trim(),
            FirstName = request.FirstName?.Trim() ?? "Super",
            LastName = request.LastName?.Trim() ?? "Admin",
            IsActive = true,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user, ct);

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = adminRole.Id,
            InstituteId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.UserRoles.AddAsync(userRole, ct);

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<TokenResponse>> SwitchSessionAsync(Guid sessionId, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Result<TokenResponse>.Failure("User not authenticated.");

        var session = await _context.AcademicSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted, ct);

        if (session is null)
            return Result<TokenResponse>.Failure("Academic session not found.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Batch)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, ct);

        if (user is null)
            return Result<TokenResponse>.Failure("User not found.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var (instituteId, tradeId, batchId) = ResolveScope(user, roles);

        if (instituteId.HasValue && session.InstituteId != instituteId.Value)
            return Result<TokenResponse>.Failure("You are not authorized to access this academic session.");

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions, instituteId, sessionId, tradeId, batchId);
        var newRefreshTokenHash = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            JwtId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };
        await _context.RefreshTokens.AddAsync(newRefreshToken, ct);
        await _context.SaveChangesAsync(ct);

        return Result<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.TokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}
