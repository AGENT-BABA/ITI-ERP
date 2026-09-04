namespace ITI.ERP.Application.DTOs.User;

public class TradeHeadDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public Guid TradeId { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string TradeCode { get; set; } = string.Empty;
}
