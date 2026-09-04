namespace ITI.ERP.Shared.Extensions;

public static class DateTimeExtensions
{
    public static string ToIndianDate(this DateTime dateTime)
    {
        return dateTime.ToString("dd/MM/yyyy");
    }

    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} month{(months == 1 ? "" : "s")} ago";
        }

        var years = (int)(timeSpan.TotalDays / 365);
        return $"{years} year{(years == 1 ? "" : "s")} ago";
    }

    public static string GetFinancialYear(this DateTime dateTime)
    {
        int startYear = dateTime.Month >= 4 ? dateTime.Year : dateTime.Year - 1;
        int endYear = startYear + 1;
        return $"{startYear}-{endYear % 100:D2}";
    }
}
