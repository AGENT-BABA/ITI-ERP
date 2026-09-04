namespace ITI.ERP.Application.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public List<string> Roles { get; set; } = new();
    public Guid? TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchName { get; set; }
    public Guid? ParentUserId { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
