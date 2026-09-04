using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Infrastructure.Persistence.Converters;
using ITI.ERP.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ITI.ERP.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTime dateTime,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _dateTime = dateTime;
        _currentUserService = currentUserService;
    }

    public DbSet<Institute> Institutes => Set<Institute>();
    public DbSet<AcademicSession> AcademicSessions => Set<AcademicSession>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<MonthlyPractical> MonthlyPracticals => Set<MonthlyPractical>();
    public DbSet<PracticalMark> PracticalMarks => Set<PracticalMark>();
    public DbSet<YearlyPractical> YearlyPracticals => Set<YearlyPractical>();
    public DbSet<YearlyPracticalMark> YearlyPracticalMarks => Set<YearlyPracticalMark>();
    public DbSet<InstituteSettings> InstituteSettings => Set<InstituteSettings>();
    public DbSet<TradeImportHistory> TradeImportHistories => Set<TradeImportHistory>();
    public DbSet<StudentImportHistory> StudentImportHistories => Set<StudentImportHistory>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = _dateTime.Now;
                entry.Entity.CreatedBy = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = _dateTime.Now;
                entry.Entity.UpdatedBy = _currentUserService.UserId;
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableDateTimeUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
