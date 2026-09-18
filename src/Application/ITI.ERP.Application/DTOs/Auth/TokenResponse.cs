namespace ITI.ERP.Application.DTOs.Auth;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
