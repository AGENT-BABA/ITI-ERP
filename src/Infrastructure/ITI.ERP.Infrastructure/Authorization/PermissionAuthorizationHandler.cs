using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ITI.ERP.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly HashSet<string> TradeHeadPermissions = new()
    {
        "Student.View", "Trade.View", "Batch.View", "AcademicSession.View",
        "Attendance.View", "Attendance.Mark",
        "Practical.View", "Practical.Create", "Practical.Lock",
        "Dashboard.View", "Reports.View", "Holiday.View",
    };

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Check individual "permission" claims (how JwtTokenService adds them)
        var allPermissionClaims = context.User?.FindAll("permission").Select(c => c.Value).ToList();
        if (allPermissionClaims != null && allPermissionClaims.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Fallback: check legacy "permissions" claim (JSON array format)
        var permissionsClaim = context.User?.FindFirst("permissions");
        if (permissionsClaim != null)
        {
            try
            {
                var permissions = System.Text.Json.JsonSerializer.Deserialize<List<string>>(permissionsClaim.Value);
                if (permissions != null && permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // Malformed permissions claim - deny access
            }
        }

        var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Admin role: grant all permissions
        if (roles != null && roles.Contains("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // TradeHead role: check scoped permissions via role-based fallback
        if (roles != null && roles.Contains("TradeHead"))
        {
            var userTradeId = context.User?.FindFirst("tradeId")?.Value;
            if (!string.IsNullOrEmpty(userTradeId) && TradeHeadPermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
