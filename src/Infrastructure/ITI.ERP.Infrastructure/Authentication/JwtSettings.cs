namespace ITI.ERP.Infrastructure.Authentication;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 15;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}
