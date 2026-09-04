using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class User : SoftDeletableEntity
{
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; }
    public Guid? ParentUserId { get; set; }

    public Institute Institute { get; set; } = null!;
    public User? ParentUser { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
