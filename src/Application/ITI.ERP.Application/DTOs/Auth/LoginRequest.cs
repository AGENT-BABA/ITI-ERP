namespace ITI.ERP.Application.DTOs.Auth;

public class LoginRequest
{
    public string GRNumber { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
