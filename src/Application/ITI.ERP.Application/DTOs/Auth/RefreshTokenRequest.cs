namespace ITI.ERP.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid? AcademicSessionId { get; set; }
}
