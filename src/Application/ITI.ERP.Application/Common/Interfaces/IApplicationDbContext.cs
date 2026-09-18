using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ITI.ERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Institute> Institutes { get; }
    DbSet<AcademicSession> AcademicSessions { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Trade> Trades { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Student> Students { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<MonthlyPractical> MonthlyPracticals { get; }
    DbSet<PracticalMark> PracticalMarks { get; }
    DbSet<YearlyPractical> YearlyPracticals { get; }
    DbSet<YearlyPracticalMark> YearlyPracticalMarks { get; }
    DbSet<InstituteSettings> InstituteSettings { get; }
    DbSet<TradeImportHistory> TradeImportHistories { get; }
    DbSet<StudentImportHistory> StudentImportHistories { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<Batch> Batches { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<ExternalLogin> ExternalLogins { get; }

    DatabaseFacade Database { get; }
    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
