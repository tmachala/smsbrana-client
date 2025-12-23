using System.Globalization;

namespace SmsBranaClient.Formatting;

internal static class QueryExtensions
{
    public static string ToQueryParam(this SmsBranaEncoding encoding)
    {
        return encoding switch
        {
            SmsBranaEncoding.Gsm7Bit => "7bit",
            SmsBranaEncoding.Ucs2 => "ucs2",
            _ => throw new ArgumentOutOfRangeException(nameof(encoding), encoding, null)
        };
    }
    
    public static string ToQueryParam(this DateTime pragueTime)
    {
        return pragueTime.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
    }

    public static string ToQueryParam(this bool flag) => flag ? "1" : "0";
    
    private static readonly TimeZoneInfo SmsConnectTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
}