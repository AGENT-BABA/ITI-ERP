using ITI.ERP.Application.DTOs.User;
using ITI.ERP.Domain.Entities;

namespace ITI.ERP.Application.Common.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Phone = u.Phone,
        ProfileImagePath = u.ProfileImagePath,
        IsActive = u.IsActive,
        IsLocked = u.IsLocked,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        TradeId = u.UserRoles.FirstOrDefault(ur => ur.TradeId != null)?.TradeId,
        BatchId = u.UserRoles.FirstOrDefault(ur => ur.BatchId != null)?.BatchId,
        BatchName = u.UserRoles.FirstOrDefault(ur => ur.Batch != null)?.Batch?.Name,
        ParentUserId = u.ParentUserId,
        LastLoginAt = u.LastLoginAt
    };
}
