using SmsBranaClient.Formatting;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class QueryExtensionsTests
{
    [Theory]
    [InlineData(SmsBranaEncoding.Gsm7Bit, "7bit")]
    [InlineData(SmsBranaEncoding.Ucs2, "ucs2")]
    public void ToQueryParam_Encoding_ReturnsCorrectString(SmsBranaEncoding encoding, string expected)
    {
        var result = encoding.ToQueryParam();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToQueryParam_Encoding_ThrowsForInvalidValue()
    {
        var invalidEncoding = (SmsBranaEncoding)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => invalidEncoding.ToQueryParam());
    }

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public void ToQueryParam_Bool_ReturnsCorrectString(bool flag, string expected)
    {
        var result = flag.ToQueryParam();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2023-12-25T14:30:45", "20231225T143045")]
    [InlineData("2024-01-01T00:00:00", "20240101T000000")]
    [InlineData("2024-06-15T23:59:59", "20240615T235959")]
    [InlineData("2024-12-31T12:00:00", "20241231T120000")]
    public void ToQueryParam_DateTime_FormatsCorrectly(string dateTimeString, string expected)
    {
        var dateTime = DateTime.Parse(dateTimeString);
        var result = dateTime.ToQueryParam();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToQueryParam_DateTime_HandlesMinValue()
    {
        var result = DateTime.MinValue.ToQueryParam();
        Assert.Equal("00010101T000000", result);
    }

    [Fact]
    public void ToQueryParam_DateTime_HandlesMaxValue()
    {
        var result = DateTime.MaxValue.ToQueryParam();
        Assert.Equal("99991231T235959", result);
    }

    [Fact]
    public void ToQueryParam_DateTime_PreservesTimeComponents()
    {
        var dateTime = new DateTime(2024, 3, 15, 9, 5, 3);
        var result = dateTime.ToQueryParam();
        Assert.Equal("20240315T090503", result);
    }

    [Fact]
    public void ToQueryParam_DateTime_HandlesLeapYear()
    {
        var dateTime = new DateTime(2024, 2, 29, 12, 30, 45);
        var result = dateTime.ToQueryParam();
        Assert.Equal("20240229T123045", result);
    }

    [Fact]
    public void ToQueryParam_DateTime_HandlesNoon()
    {
        var dateTime = new DateTime(2024, 6, 1, 12, 0, 0);
        var result = dateTime.ToQueryParam();
        Assert.Equal("20240601T120000", result);
    }

    [Fact]
    public void ToQueryParam_DateTime_HandlesMidnight()
    {
        var dateTime = new DateTime(2024, 6, 1, 0, 0, 0);
        var result = dateTime.ToQueryParam();
        Assert.Equal("20240601T000000", result);
    }
}
