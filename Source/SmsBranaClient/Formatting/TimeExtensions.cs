namespace SmsBranaClient.Formatting;

public static class TimeExtensions
{
    public static DateTime UtcToPragueTime(this DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, PragueTimeZone);
    }
    
    private static readonly TimeZoneInfo PragueTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
}