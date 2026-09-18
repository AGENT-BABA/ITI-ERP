using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, List<string> roles, List<string> permissions, Guid? instituteId = null, Guid? academicSessionId = null, Guid? tradeId = null, Guid? batchId = null);
    string GenerateRefreshToken();
    int GetAccessTokenExpiryInMinutes();
}
