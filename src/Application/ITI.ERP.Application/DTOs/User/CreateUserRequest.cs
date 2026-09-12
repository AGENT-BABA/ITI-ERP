namespace ITI.ERP.Application.DTOs.User;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
    public Guid? InstituteId { get; set; }
    public Guid? TradeId { get; set; }
    public Guid? BatchId { get; set; }
}
