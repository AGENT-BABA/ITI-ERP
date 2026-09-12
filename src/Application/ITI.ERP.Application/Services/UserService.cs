using System.Security.Cryptography;
using System.Text;
using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.User;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Application.Configuration;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ITI.ERP.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly EmailOptions _emailOptions;

    public UserService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IEmailService emailService,
        IOptions<EmailOptions> emailOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _emailService = emailService;
        _emailOptions = emailOptions.Value;
    }

    public async Task<Result<PaginatedList<UserDto>>> GetUsersAsync(PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<User> query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Batch);

        if (isSuperAdmin)
        {
            query = query.Where(u => !u.IsDeleted &&
                u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == RoleConstants.InstituteAdmin));
        }
        else
        {
            query = query.Where(u =>
                u.InstituteId == _currentUserService.InstituteId && !u.IsDeleted &&
                u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == RoleConstants.TradeHead));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "username" => request.SortDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "firstname" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            _ => query.OrderBy(u => u.Username)
        };

        var paginatedList = await PaginatedList<UserDto>.CreateAsync(
            query.Select(u => u.ToDto()),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<UserDto>>.Success(paginatedList);
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Batch)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result<UserDto>.Failure("User not found.");

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);

        if (!isSuperAdmin && !isInstituteAdmin)
            return Result<UserDto>.Failure("Only SuperAdmin or InstituteAdmin can create users.");

        if (!request.RoleIds.Any())
            return Result<UserDto>.Failure("At least one role must be assigned.");

        var requestedRoleNames = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(ct);

        if (isSuperAdmin)
        {
            if (requestedRoleNames.Any(r => r != RoleConstants.InstituteAdmin))
                return Result<UserDto>.Failure("SuperAdmin can only assign InstituteAdmin role.");

            if (!request.InstituteId.HasValue)
                return Result<UserDto>.Failure("InstituteId is required when SuperAdmin creates an InstituteAdmin.");

            if (!await _context.Institutes.AnyAsync(i => i.Id == request.InstituteId.Value && i.IsActive, ct))
                return Result<UserDto>.Failure("Invalid or inactive institute.");
        }
        else
        {
            if (requestedRoleNames.Any(r => r != RoleConstants.TradeHead))
                return Result<UserDto>.Failure("InstituteAdmin can only assign TradeHead role.");

            if (!request.TradeId.HasValue)
                return Result<UserDto>.Failure("TradeId is required when InstituteAdmin creates a TradeHead.");

            if (!request.BatchId.HasValue)
                return Result<UserDto>.Failure("BatchId is required when InstituteAdmin creates a TradeHead.");

            var tradeBelongsToInstitute = await _context.Trades.AnyAsync(
                t => t.Id == request.TradeId.Value && t.InstituteId == _currentUserService.InstituteId, ct);

            if (!tradeBelongsToInstitute)
                return Result<UserDto>.Failure("Trade does not belong to your institute.");

            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.Id == request.BatchId.Value && !b.IsDeleted, ct);

            if (batch is null)
                return Result<UserDto>.Failure("Batch not found.");

            if (batch.InstituteId != _currentUserService.InstituteId)
                return Result<UserDto>.Failure("Batch does not belong to your institute.");

            if (!batch.IsActive)
                return Result<UserDto>.Failure("Cannot assign TradeHead to an archived batch.");

            if (batch.TradeId != request.TradeId.Value)
                return Result<UserDto>.Failure("Batch does not belong to the specified trade.");

            var existingTradeHead = await _context.UserRoles
                .Include(ur => ur.User)
                .AnyAsync(ur => ur.Role.Name == RoleConstants.TradeHead
                    && ur.TradeId == request.TradeId.Value
                    && ur.BatchId == request.BatchId.Value
                    && ur.IsActive
                    && !ur.User.IsDeleted, ct);

            if (existingTradeHead)
                return Result<UserDto>.Failure("This batch already has an active TradeHead assigned.");
        }

        Guid instituteId;
        if (isSuperAdmin)
        {
            if (!request.InstituteId.HasValue)
                return Result<UserDto>.Failure("InstituteId is required when SuperAdmin creates a user.");
            instituteId = request.InstituteId.Value;
        }
        else
        {
            if (!_currentUserService.InstituteId.HasValue)
                return Result<UserDto>.Failure("Institute not found.");
            instituteId = _currentUserService.InstituteId.Value;
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.InstituteId == instituteId && !u.IsDeleted, ct))
            return Result<UserDto>.Failure("Username already exists in this institute.");

        string email = request.Email.Trim();

        if (await _context.Users.AnyAsync(u => u.Email == email && u.InstituteId == instituteId && !u.IsDeleted, ct))
            return Result<UserDto>.Failure($"A user with email '{email}' already exists in this institute.");

        var user = new User
        {
            InstituteId = instituteId,
            Username = request.Username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password , 12),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            IsActive = true,
            IsLocked = false,
            ParentUserId = isInstituteAdmin && requestedRoleNames.Any(r => r == RoleConstants.TradeHead)
                ? _currentUserService.UserId
                : null
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Users.AddAsync(user, ct);

            if (request.RoleIds.Any())
            {
                var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

                var userRoles = request.RoleIds.Select(roleId => new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    InstituteId = instituteId,
                    AcademicSessionId = academicSessionId,
                    IsActive = true,
                    TradeId = request.TradeId,
                    BatchId = request.BatchId
                });

                await _context.UserRoles.AddRangeAsync(userRoles, ct);
            }

            await _context.SaveChangesAsync(ct);

            var roles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);

            await _auditService.LogAsync(Domain.Enums.AuditAction.Create, nameof(User), user.Id, null, new { user.Username, user.Email, user.FirstName }, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            user.UserRoles = request.RoleIds.Select(roleId => new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                Role = new Role { Id = roleId, Name = roles.FirstOrDefault() ?? string.Empty }
            }).ToList();

            return Result<UserDto>.Success(user.ToDto());
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);
        if (!isSuperAdmin && !isInstituteAdmin)
            return Result<UserDto>.Failure("Only SuperAdmin or InstituteAdmin can update users.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result<UserDto>.Failure("User not found.");

        if (isInstituteAdmin && !user.UserRoles.Any(ur => ur.Role.Name == RoleConstants.TradeHead))
            return Result<UserDto>.Failure("InstituteAdmin can only update TradeHead users.");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id && !u.IsDeleted, ct))
            return Result<UserDto>.Failure("Email already exists.");

        var oldValues = new
        {
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;

            if (request.RoleIds is not null)
            {
                if (isInstituteAdmin)
                {
                    var requestedRoleNames = await _context.Roles
                        .Where(r => request.RoleIds.Contains(r.Id))
                        .Select(r => r.Name)
                        .ToListAsync(ct);

                    if (requestedRoleNames.Any(roleName => roleName != RoleConstants.TradeHead))
                        return Result<UserDto>.Failure("InstituteAdmin can only assign the TradeHead role.");

                    if (request.TradeId.HasValue && request.BatchId.HasValue)
                    {
                        var batch = await _context.Batches
                            .FirstOrDefaultAsync(b => b.Id == request.BatchId.Value && !b.IsDeleted, ct);

                        if (batch is null)
                            return Result<UserDto>.Failure("Batch not found.");

                        if (batch.InstituteId != _currentUserService.InstituteId)
                            return Result<UserDto>.Failure("Batch does not belong to your institute.");

                        if (!batch.IsActive)
                            return Result<UserDto>.Failure("Cannot assign TradeHead to an archived batch.");

                        if (batch.TradeId != request.TradeId.Value)
                            return Result<UserDto>.Failure("Batch does not belong to the specified trade.");

                        var existingTradeHead = await _context.UserRoles
                            .Include(ur => ur.User)
                            .AnyAsync(ur => ur.Role.Name == RoleConstants.TradeHead
                                && ur.TradeId == request.TradeId.Value
                                && ur.BatchId == request.BatchId.Value
                                && ur.IsActive
                                && ur.UserId != id
                                && !ur.User.IsDeleted, ct);

                        if (existingTradeHead)
                            return Result<UserDto>.Failure("This batch already has an active TradeHead assigned.");
                    }
                }

                var existingUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .ToListAsync(ct);

                var preservedTradeId = request.TradeId ?? existingUserRoles.FirstOrDefault()?.TradeId;
                var preservedBatchId = request.BatchId ?? existingUserRoles.FirstOrDefault()?.BatchId;

                _context.UserRoles.RemoveRange(existingUserRoles);

                var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

                var userInstituteId = isSuperAdmin
                    ? user.InstituteId
                    : _currentUserService.InstituteId;

                if (userInstituteId is null)
                    return Result<UserDto>.Failure(isSuperAdmin ? "User has no institute assigned." : "Institute not found.");

                var newUserRoles = request.RoleIds.Select(roleId => new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    InstituteId = userInstituteId,
                    AcademicSessionId = academicSessionId,
                    IsActive = true,
                    TradeId = preservedTradeId,
                    BatchId = preservedBatchId
                });

                await _context.UserRoles.AddRangeAsync(newUserRoles, ct);
            }

            await _context.SaveChangesAsync(ct);

            var roles = await _context.Roles
                .Where(r => (request.RoleIds ?? user.UserRoles.Select(ur => ur.RoleId)).Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);

            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(User), user.Id, oldValues, new { user.Email, user.FirstName, user.LastName }, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return Result<UserDto>.Success(user!.ToDto());
    }

    public async Task<Result> ToggleUserStatusAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);
        if (!isSuperAdmin && !isInstituteAdmin)
            return Result.Failure("Only SuperAdmin or InstituteAdmin can update user status.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result.Failure("User not found.");

        if (isInstituteAdmin && !await _context.UserRoles.AnyAsync(ur => ur.UserId == id && ur.Role.Name == RoleConstants.TradeHead, ct))
            return Result.Failure("InstituteAdmin can only manage TradeHead users.");

        var oldStatus = user.IsActive;
        user.IsActive = !user.IsActive;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(User), user.Id, new { IsActive = oldStatus }, new { IsActive = user.IsActive }, ct);

        return Result.Success();
    }

    public async Task<Result> SendPasswordResetAsync(Guid userId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);
        if (!isSuperAdmin && !isInstituteAdmin)
            return Result.Failure("Only SuperAdmin or InstituteAdmin can send password resets.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result.Failure("User not found.");

        if (isInstituteAdmin && !await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.Role.Name == RoleConstants.TradeHead, ct))
            return Result.Failure("InstituteAdmin can only manage TradeHead users.");

        if (string.IsNullOrEmpty(user.Email))
            return Result.Failure("User has no email address. Cannot send password reset.");

        return await CreateAndSendResetTokenAsync(user.Id, user.Email, user.FirstName, user.InstituteId, _currentUserService.UserId, ct);
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail && !u.IsDeleted && u.IsActive, ct);

        if (user is null)
            return Result.Success();

        var recentTokenExists = await _context.PasswordResetTokens
            .AnyAsync(t => t.UserId == user.Id
                && !t.UsedAt.HasValue
                && t.ExpiresAt > DateTime.UtcNow
                && t.CreatedAt > DateTime.UtcNow.AddMinutes(-1), ct);

        if (recentTokenExists)
            return Result.Success();

        return await CreateAndSendResetTokenAsync(user.Id, user.Email!, user.FirstName, user.InstituteId, null, ct);
    }

    public async Task<Result> VerifyResetTokenAsync(string token, CancellationToken ct)
    {
        var tokenHash = HashToken(token);

        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (resetToken is null || resetToken.UsedAt.HasValue || resetToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure("Invalid or expired reset token.");

        return Result.Success();
    }

    public async Task<Result> ResetPasswordWithTokenAsync(string token, string newPassword, CancellationToken ct)
    {
        var tokenHash = HashToken(token);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

            if (resetToken is null || resetToken.UsedAt.HasValue || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure("Invalid or expired reset token.");
            }

            var markedCount = await _context.PasswordResetTokens
                .Where(t => t.Id == resetToken.Id && !t.UsedAt.HasValue)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);

            if (markedCount == 0)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure("Invalid or expired reset token.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == resetToken.UserId && !u.IsDeleted, ct);

            if (user is null)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure("User not found.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword , 12);
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.IsActive)
                .ToListAsync(ct);

            foreach (var rt in activeRefreshTokens)
            {
                rt.IsActive = false;
                rt.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(
                AuditAction.PasswordResetCompleted,
                nameof(User),
                user.Id,
                null,
                new { UserEmail = user.Email },
                ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<Result> CreateAndSendResetTokenAsync(
        Guid userId, string email, string firstName, Guid? instituteId, Guid? initiatedByUserId, CancellationToken ct)
    {
        await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.UsedAt.HasValue)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);

        var rawToken = GenerateSecureToken();
        var tokenHash = HashToken(rawToken);

        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow,
            InitiatedByUserId = initiatedByUserId,
            InstituteId = instituteId
        };

        await _context.PasswordResetTokens.AddAsync(resetToken, ct);
        await _context.SaveChangesAsync(ct);

        var resetLink = $"{_emailOptions.FrontendBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var instituteName = instituteId.HasValue
            ? await _context.Institutes
                .Where(i => i.Id == instituteId.Value)
                .Select(i => i.Name)
                .FirstOrDefaultAsync(ct)
            : null;

        await _emailService.SendPasswordResetEmailAsync(email, firstName, resetLink, instituteName, ct);

        await _auditService.LogAsync(
            AuditAction.PasswordResetRequested,
            nameof(User),
            userId,
            null,
            new { InitiatedBySuperAdmin = initiatedByUserId.HasValue },
            ct);

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string GenerateSecureToken()
    {
        byte[] tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task<Result> UnlockUserAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);
        if (!isSuperAdmin && !isInstituteAdmin)
            return Result.Failure("Only SuperAdmin or InstituteAdmin can unlock users.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result.Failure("User not found.");

        if (isInstituteAdmin && !await _context.UserRoles.AnyAsync(ur => ur.UserId == id && ur.Role.Name == RoleConstants.TradeHead, ct))
            return Result.Failure("InstituteAdmin can only manage TradeHead users.");

        if (!user.IsLocked)
            return Result.Failure("User is not locked.");

        user.IsLocked = false;
        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(User), user.Id, new { IsLocked = true }, new { IsLocked = false }, ct);

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);
        if (!isSuperAdmin && !isInstituteAdmin)
            return Result.Failure("Only SuperAdmin or InstituteAdmin can delete users.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted &&
                (isSuperAdmin || u.InstituteId == _currentUserService.InstituteId), ct);

        if (user is null)
            return Result.Failure("User not found.");

        var targetRole = isSuperAdmin ? RoleConstants.InstituteAdmin : RoleConstants.TradeHead;
        if (!user.UserRoles.Any(ur => ur.Role?.Name == targetRole))
            return Result.Failure(isSuperAdmin
                ? "Only InstituteAdmin users can be deleted."
                : "InstituteAdmin can only delete TradeHead users.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var childrenDeleted = 0;

            if (isSuperAdmin && user.UserRoles.Any(ur => ur.Role?.Name == RoleConstants.InstituteAdmin))
            {
                var childUsers = await _context.Users
                    .Where(u => u.ParentUserId == id && !u.IsDeleted)
                    .ToListAsync(ct);

                foreach (var child in childUsers)
                {
                    child.IsDeleted = true;
                    child.IsActive = false;
                    childrenDeleted++;

                    await _auditService.LogAsync(
                        Domain.Enums.AuditAction.Delete,
                        nameof(User),
                        child.Id,
                        new { child.Username, child.Email, DeletedWithParent = true },
                        null,
                        ct);
                }
            }

            user.IsDeleted = true;
            user.IsActive = false;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(
                Domain.Enums.AuditAction.Delete,
                nameof(User),
                user.Id,
                new { user.Username, user.Email, ChildrenDeleted = childrenDeleted },
                null,
                ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Result<List<TradeHeadDto>>> GetTradeHeadsByInstituteAsync(Guid instituteId, CancellationToken ct)
    {
        if (!_currentUserService.HasRole(RoleConstants.Admin))
            return Result<List<TradeHeadDto>>.Failure("Only SuperAdmin can view trade heads.");

        var tradeHeads = await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Include(ur => ur.Trade)
            .Where(ur =>
                ur.User != null &&
                !ur.User.IsDeleted &&
                ur.User.InstituteId == instituteId &&
                ur.Role != null &&
                ur.Role.Name == "TradeHead")
            .Select(ur => new TradeHeadDto
            {
                UserId = ur.User!.Id,
                Username = ur.User.Username,
                Email = ur.User.Email,
                FirstName = ur.User.FirstName,
                LastName = ur.User.LastName,
                IsActive = ur.User.IsActive,
                IsLocked = ur.User.IsLocked,
                TradeId = ur.TradeId ?? Guid.Empty,
                TradeName = ur.Trade != null ? ur.Trade.Name : string.Empty,
                TradeCode = ur.Trade != null ? ur.Trade.Code : string.Empty,
            })
            .ToListAsync(ct);

        return Result<List<TradeHeadDto>>.Success(tradeHeads);
    }
}
