using System.Security.Claims;
using ITI.ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ITI.ERP.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : null;
        }
    }

    public Guid? InstituteId
    {
        get
        {
            var instituteIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("instituteId");
            return instituteIdClaim != null ? Guid.Parse(instituteIdClaim.Value) : null;
        }
    }

    public Guid? AcademicSessionId
    {
        get
        {
            var academicSessionIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("academicSessionId");
            return academicSessionIdClaim != null ? Guid.Parse(academicSessionIdClaim.Value) : null;
        }
    }

    public Guid? TradeId
    {
        get
        {
            var tradeIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tradeId");
            return tradeIdClaim != null ? Guid.Parse(tradeIdClaim.Value) : null;
        }
    }

    public Guid? BatchId
    {
        get
        {
            var batchIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("batchId");
            return batchIdClaim != null ? Guid.Parse(batchIdClaim.Value) : null;
        }
    }

    public string? UserName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public bool HasRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}