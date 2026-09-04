namespace ITI.ERP.Application.Configuration;

public class EmailOptions
{
    public string Provider { get; set; } = "Console";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
}
