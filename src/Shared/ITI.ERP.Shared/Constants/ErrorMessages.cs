namespace ITI.ERP.Shared.Constants;

public static class ErrorMessages
{
    public const string IncorrectCredentials = "Incorrect email or password.";
    public const string GenericRequestFailed = "Request failed.";
    public const string InvalidOrExpiredToken = "Invalid or expired reset token.";
    public const string Unauthorized = "You are not authorized to perform this action.";
    public const string NotFound = "The requested resource was not found.";
    public const string Forbidden = "You do not have permission to access this resource.";
    public const string ValidationFailed = "Validation failed. Please check your input.";
    public const string DuplicateEntry = "A record with the same value already exists.";
    public const string SessionExpired = "Your session has expired. Please log in again.";
    public const string TokenRevoked = "Your token has been revoked. Please log in again.";
    public const string TooManyAttempts = "Too many requests. Please try again later.";
}
