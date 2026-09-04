namespace ITI.ERP.Application.DTOs.User;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public Guid? TradeId { get; set; }
    public Guid? BatchId { get; set; }
}
