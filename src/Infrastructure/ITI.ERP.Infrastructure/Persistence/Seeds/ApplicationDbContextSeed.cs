using ITI.ERP.Domain.Constants;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ITI.ERP.Infrastructure.Persistence.Seeds;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(IServiceScopeFactory serviceScopeFactory)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Log.Information("Starting database seed...");

            await SeedPermissionsAsync(context);
            await SeedRolesAsync(context);

            Log.Information("Database seed completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database seed failed with exception.");
        }
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        try
        {
            var permissionCodes = new List<string>
            {
                Permissions.Institute.View,
                Permissions.Institute.Edit,
                Permissions.Institute.Delete,
                Permissions.AcademicSession.View,
                Permissions.AcademicSession.Create,
                Permissions.AcademicSession.Edit,
                Permissions.AcademicSession.Delete,
                Permissions.AcademicSession.Lock,
                Permissions.AcademicSession.Activate,
                Permissions.Trade.View,
                Permissions.Trade.Create,
                Permissions.Trade.Edit,
                Permissions.Trade.Archive,
                Permissions.Trade.AssignHead,
                Permissions.Trade.Import,
                Permissions.Student.View,
                Permissions.Student.Create,
                Permissions.Student.Edit,
                Permissions.Student.Delete,
                Permissions.Student.Archive,
                Permissions.Student.Transfer,
                Permissions.Student.Photo,
                Permissions.Student.Import,
                Permissions.Attendance.View,
                Permissions.Attendance.Mark,
                Permissions.Attendance.Unlock,
                Permissions.Attendance.Export,
                Permissions.Practical.View,
                Permissions.Practical.Create,
                Permissions.Practical.Edit,
                Permissions.Practical.Delete,
                Permissions.Practical.Lock,
                Permissions.Practical.Unlock,
                Permissions.Reports.View,
                Permissions.Reports.Export,
                Permissions.Users.View,
                Permissions.Users.Manage,
                Permissions.Users.Edit,
                Permissions.Users.ToggleStatus,
                Permissions.Users.CreateTradeHead,
                Permissions.Users.ResetPassword,
                Permissions.Users.Unlock,
                Permissions.Settings.Manage,
                Permissions.Dashboard.View,
                Permissions.Holiday.View,
                Permissions.Holiday.Create,
                Permissions.Holiday.Delete,
                Permissions.Batch.View,
                Permissions.Batch.Create,
                Permissions.Batch.Edit,
                Permissions.Batch.Archive
            };

            var existingCodes = new HashSet<string>(
                await context.Permissions.Select(p => p.Code).ToListAsync());

            var missing = permissionCodes
                .Where(code => !existingCodes.Contains(code))
                .ToList();

            if (missing.Count == 0)
            {
                Log.Information("All {Total} permissions already exist, nothing to add.", permissionCodes.Count);
                return;
            }

            var now = DateTime.UtcNow;
            var newPermissions = missing.Select(code =>
            {
                var parts = code.Split('.');
                return new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Module = parts[0],
                    Action = parts[1],
                    Description = code,
                    IsActive = true,
                    CreatedAt = now
                };
            }).ToList();

            context.Permissions.AddRange(newPermissions);
            var count = await context.SaveChangesAsync();
            Log.Information("Added {Added} new permissions ({Total} total in master list)", count, permissionCodes.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to seed Permissions.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        try
        {
            var allPermissions = await context.Permissions.ToListAsync();
            var allPermissionIds = new HashSet<Guid>(allPermissions.Select(p => p.Id));
            var now = DateTime.UtcNow;

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole is null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    InstituteId = null,
                    Name = "Admin",
                    RoleType = RoleType.Admin,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now
                };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
                Log.Information("Created Admin role.");
            }

            var existingAdminPermissionIds = new HashSet<Guid>(
                await context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync());

            var adminMissing = allPermissionIds
                .Where(pid => !existingAdminPermissionIds.Contains(pid))
                .Select(pid => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    PermissionId = pid,
                    IsActive = true,
                    CreatedAt = now
                })
                .ToList();

            if (adminMissing.Count > 0)
            {
                context.RolePermissions.AddRange(adminMissing);
                await context.SaveChangesAsync();
                Log.Information("Admin role: added {Added} missing permissions.", adminMissing.Count);
            }
            else
            {
                Log.Information("Admin role: all {Total} permissions already assigned.", allPermissionIds.Count);
            }

            var instituteAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "InstituteAdmin");
            if (instituteAdminRole is null)
            {
                instituteAdminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    InstituteId = null,
                    Name = "InstituteAdmin",
                    RoleType = RoleType.InstituteAdmin,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now
                };
                context.Roles.Add(instituteAdminRole);
                await context.SaveChangesAsync();
                Log.Information("Created InstituteAdmin role.");
            }

            var instituteAdminPermissionCodes = new HashSet<string>
            {
                Permissions.Dashboard.View,
                Permissions.Institute.View,
                Permissions.Trade.View,
                Permissions.Trade.Create,
                Permissions.Trade.Edit,
                Permissions.Student.View,
                Permissions.Student.Create,
                Permissions.Student.Edit,
                Permissions.Student.Delete,
                Permissions.Student.Archive,
                Permissions.Student.Photo,
                Permissions.Student.Import,
                Permissions.Student.Transfer,
                Permissions.Users.View,
                Permissions.Users.Manage,
                Permissions.Users.Edit,
                Permissions.Users.ToggleStatus,
                Permissions.Users.Unlock,
                Permissions.Settings.Manage,
                Permissions.Reports.View,
                Permissions.Holiday.View,
                Permissions.Holiday.Create,
                Permissions.Holiday.Delete,
                Permissions.Batch.View,
                Permissions.Batch.Create,
                Permissions.Batch.Edit,
                Permissions.Batch.Archive,
                Permissions.AcademicSession.View,
                Permissions.AcademicSession.Create,
                Permissions.AcademicSession.Edit,
                Permissions.AcademicSession.Delete,
                Permissions.AcademicSession.Lock,
                Permissions.AcademicSession.Activate
            };

            var instituteAdminTargetIds = new HashSet<Guid>(
                allPermissions
                    .Where(p => instituteAdminPermissionCodes.Contains(p.Code))
                    .Select(p => p.Id));

            var existingInstituteAdminEntries = await context.RolePermissions
                .Where(rp => rp.RoleId == instituteAdminRole.Id)
                .ToListAsync();

            var existingInstituteAdminPermissionIds = new HashSet<Guid>(
                existingInstituteAdminEntries.Select(rp => rp.PermissionId));

            var iaToAdd = instituteAdminTargetIds
                .Where(pid => !existingInstituteAdminPermissionIds.Contains(pid))
                .Select(pid => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = instituteAdminRole.Id,
                    PermissionId = pid,
                    IsActive = true,
                    CreatedAt = now
                })
                .ToList();

            var iaToRemove = existingInstituteAdminEntries
                .Where(rp => !instituteAdminTargetIds.Contains(rp.PermissionId))
                .ToList();

            if (iaToAdd.Count > 0 || iaToRemove.Count > 0)
            {
                if (iaToAdd.Count > 0)
                    context.RolePermissions.AddRange(iaToAdd);
                if (iaToRemove.Count > 0)
                    context.RolePermissions.RemoveRange(iaToRemove);

                await context.SaveChangesAsync();
                Log.Information("InstituteAdmin role: added {Added}, removed {Removed} permissions.",
                    iaToAdd.Count, iaToRemove.Count);
            }
            else
            {
                Log.Information("InstituteAdmin role: permissions already in sync ({Total} assigned).",
                    instituteAdminTargetIds.Count);
            }

            var tradeHeadRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "TradeHead");
            if (tradeHeadRole is null)
            {
                tradeHeadRole = new Role
                {
                    Id = Guid.NewGuid(),
                    InstituteId = null,
                    Name = "TradeHead",
                    RoleType = RoleType.TradeHead,
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = now
                };
                context.Roles.Add(tradeHeadRole);
                await context.SaveChangesAsync();
                Log.Information("Created TradeHead role.");
            }

            var tradeHeadPermissionCodes = new HashSet<string>
            {
                Permissions.Student.View,
                Permissions.Trade.View,
                Permissions.Batch.View,
                Permissions.AcademicSession.View,
                Permissions.Attendance.View,
                Permissions.Attendance.Mark,
                Permissions.Practical.View,
                Permissions.Practical.Create,
                Permissions.Practical.Delete,
                Permissions.Practical.Lock,
                Permissions.Dashboard.View,
                Permissions.Reports.View,
                Permissions.Holiday.View
            };

            var tradeHeadTargetIds = new HashSet<Guid>(
                allPermissions
                    .Where(p => tradeHeadPermissionCodes.Contains(p.Code))
                    .Select(p => p.Id));

            var existingTradeHeadEntries = await context.RolePermissions
                .Where(rp => rp.RoleId == tradeHeadRole.Id)
                .ToListAsync();

            var existingTradeHeadPermissionIds = new HashSet<Guid>(
                existingTradeHeadEntries.Select(rp => rp.PermissionId));

            var toAdd = tradeHeadTargetIds
                .Where(pid => !existingTradeHeadPermissionIds.Contains(pid))
                .Select(pid => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = tradeHeadRole.Id,
                    PermissionId = pid,
                    IsActive = true,
                    CreatedAt = now
                })
                .ToList();

            var toRemove = existingTradeHeadEntries
                .Where(rp => !tradeHeadTargetIds.Contains(rp.PermissionId))
                .ToList();

            if (toAdd.Count > 0 || toRemove.Count > 0)
            {
                if (toAdd.Count > 0)
                    context.RolePermissions.AddRange(toAdd);
                if (toRemove.Count > 0)
                    context.RolePermissions.RemoveRange(toRemove);

                await context.SaveChangesAsync();
                Log.Information("TradeHead role: added {Added}, removed {Removed} permissions.",
                    toAdd.Count, toRemove.Count);
            }
            else
            {
                Log.Information("TradeHead role: permissions already in sync ({Total} assigned).",
                    tradeHeadTargetIds.Count);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to seed Roles.");
            throw;
        }
    }
}
