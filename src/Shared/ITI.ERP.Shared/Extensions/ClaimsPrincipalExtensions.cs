using System.Security.Claims;
using ITI.ERP.Shared.Constants;

namespace ITI.ERP.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimConstants.UserId);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }

    public static Guid? GetInstituteId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimConstants.InstituteId);
        return claim != null && Guid.TryParse(claim.Value, out var instituteId) ? instituteId : null;
    }

    public static Guid? GetAcademicSessionId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimConstants.AcademicSessionId);
        return claim != null && Guid.TryParse(claim.Value, out var sessionId) ? sessionId : null;
    }

    public static Guid? GetTradeId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimConstants.TradeId);
        return claim != null && Guid.TryParse(claim.Value, out var tradeId) ? tradeId : null;
    }

    public static List<string> GetPermissions(this ClaimsPrincipal principal)
    {
        var claims = principal.FindAll(ClaimConstants.Permissions);
        return claims.Select(c => c.Value).ToList();
    }

    public static string? GetRole(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.Role);
        return claim?.Value;
    }
}
