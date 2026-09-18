namespace ITI.ERP.Application.DTOs.Auth;

/// <summary>
/// Internal result containing the raw refresh token value for cookie transport.
/// SECURITY: This type must NEVER be serialized to API JSON responses.
/// The controller must extract RawToken to set an HttpOnly cookie and discard it.
/// RawToken must never appear in logs, exception messages, or diagnostic output.
/// </summary>
public class RefreshTokenResult
{
    public string RawToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
