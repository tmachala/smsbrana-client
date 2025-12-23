using SmsBranaClient.Formatting;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class TimeExtensionsTests
{
    [Fact]
    public void UtcToPragueTime_ConvertsCorrectly_WinterTime()
    {
        // Prague is UTC+1 in winter (standard time)
        var utc = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(2024, pragueTime.Year);
        Assert.Equal(1, pragueTime.Month);
        Assert.Equal(15, pragueTime.Day);
        Assert.Equal(13, pragueTime.Hour); // UTC+1
        Assert.Equal(0, pragueTime.Minute);
        Assert.Equal(0, pragueTime.Second);
    }

    [Fact]
    public void UtcToPragueTime_ConvertsCorrectly_SummerTime()
    {
        // Prague is UTC+2 in summer (daylight saving time)
        var utc = new DateTime(2024, 7, 15, 12, 0, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(2024, pragueTime.Year);
        Assert.Equal(7, pragueTime.Month);
        Assert.Equal(15, pragueTime.Day);
        Assert.Equal(14, pragueTime.Hour); // UTC+2
        Assert.Equal(0, pragueTime.Minute);
        Assert.Equal(0, pragueTime.Second);
    }

    [Fact]
    public void UtcToPragueTime_HandlesMidnight()
    {
        var utc = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(1, pragueTime.Hour); // UTC+1 in winter
    }

    [Fact]
    public void UtcToPragueTime_HandlesDateChange()
    {
        // 23:00 UTC on Jan 15 becomes 00:00 Prague time on Jan 16 (winter)
        var utc = new DateTime(2024, 1, 15, 23, 0, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(16, pragueTime.Day);
        Assert.Equal(0, pragueTime.Hour);
    }

    [Fact]
    public void UtcToPragueTime_HandlesYearChange()
    {
        // Last hour of 2023 UTC becomes first hour of 2024 Prague time
        var utc = new DateTime(2023, 12, 31, 23, 30, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(2024, pragueTime.Year);
        Assert.Equal(1, pragueTime.Month);
        Assert.Equal(1, pragueTime.Day);
        Assert.Equal(0, pragueTime.Hour);
        Assert.Equal(30, pragueTime.Minute);
    }

    [Fact]
    public void UtcToPragueTime_PreservesMinutesAndSeconds()
    {
        var utc = new DateTime(2024, 6, 15, 14, 45, 30, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(45, pragueTime.Minute);
        Assert.Equal(30, pragueTime.Second);
    }

    [Fact]
    public void UtcToPragueTime_HandlesDstTransition_Spring()
    {
        // Just before DST starts in 2024 (last Sunday of March, 2:00 AM becomes 3:00 AM)
        // March 31, 2024 at 01:00 UTC is still 02:00 CET (before transition)
        var utcBeforeDst = new DateTime(2024, 3, 31, 0, 30, 0, DateTimeKind.Utc);
        var pragueBeforeDst = utcBeforeDst.UtcToPragueTime();
        
        // Should be CET (UTC+1)
        Assert.Equal(1, pragueBeforeDst.Hour);
        Assert.Equal(30, pragueBeforeDst.Minute);
    }

    [Fact]
    public void UtcToPragueTime_HandlesDstTransition_Autumn()
    {
        // Just before DST ends in 2024 (last Sunday of October, 3:00 AM becomes 2:00 AM)
        // October 27, 2024 at 00:30 UTC is still 02:30 CEST (before transition)
        var utcBeforeDst = new DateTime(2024, 10, 27, 0, 30, 0, DateTimeKind.Utc);
        var pragueBeforeDst = utcBeforeDst.UtcToPragueTime();
        
        // Should be CEST (UTC+2)
        Assert.Equal(2, pragueBeforeDst.Hour);
        Assert.Equal(30, pragueBeforeDst.Minute);
    }

    [Fact]
    public void UtcToPragueTime_HandlesLeapYear()
    {
        // Feb 29, 2024 (leap year)
        var utc = new DateTime(2024, 2, 29, 12, 0, 0, DateTimeKind.Utc);
        var pragueTime = utc.UtcToPragueTime();
        
        Assert.Equal(29, pragueTime.Day);
        Assert.Equal(2, pragueTime.Month);
        Assert.Equal(13, pragueTime.Hour); // UTC+1 in February
    }
}
