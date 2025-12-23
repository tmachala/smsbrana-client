using SmsBranaClient.Encoding;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class Ucs2EncodingTests
{
    [Theory]
    [InlineData("Hello World", 11)]
    [InlineData("ščřžýáíé", 8)]
    [InlineData("", 0)]
    [InlineData("😊", 2)] // Emoji is a surrogate pair, so the length is 2 chars
    [InlineData("Hello\nWorld", 11)] // Line feed counts as 1 char
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 26)]
    [InlineData("€100", 4)]
    [InlineData("Čau! Café costs €5", 18)]
    public void GetMessageLength_CalculatesCorrectly(string input, int expectedLength)
    {
        var length = Ucs2Encoding.GetMessageLength(input);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData("Hello\r\nWorld", 11)] // Windows line ending normalized to \n
    [InlineData("Hello\rWorld", 11)] // Old Mac line ending normalized to \n
    [InlineData("Line1\r\nLine2\r\nLine3", 17)] // Multiple Windows line endings
    public void GetMessageLength_HandlesLineEndings(string input, int expectedLength)
    {
        var length = Ucs2Encoding.GetMessageLength(input);
        Assert.Equal(expectedLength, length);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 1)]
    [InlineData(70, 1)] // Max for single message
    [InlineData(71, 2)] // First multipart
    [InlineData(134, 2)] // 67 * 2
    [InlineData(135, 3)]
    [InlineData(201, 3)] // 67 * 3
    [InlineData(202, 4)]
    [InlineData(1, 1)]
    [InlineData(67, 1)] // Multipart boundary
    public void EstimateMessageCount_CalculatesCorrectly(int length, int expectedCount)
    {
        var count = Ucs2Encoding.EstimateMessageCount(length);
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void GetMessageLength_HandlesEmojis()
    {
        // Multiple emojis
        var result = Ucs2Encoding.GetMessageLength("😊😀🎉");
        Assert.Equal(6, result); // Each emoji is 2 chars (surrogate pair)
    }

    [Fact]
    public void GetMessageLength_HandlesMixedContent()
    {
        var text = "Hello 😊 World!";
        var length = Ucs2Encoding.GetMessageLength(text);
        Assert.Equal(15, length); // 'Hello ' (6) + '😊' (2) + ' World!' (7)
    }

    [Fact]
    public void EstimateMessageCount_SingleCharacter()
    {
        var count = Ucs2Encoding.EstimateMessageCount(1);
        Assert.Equal(1, count);
    }

    [Fact]
    public void EstimateMessageCount_ExactlyAtBoundary()
    {
        // Exactly 70 characters should be 1 message
        var count70 = Ucs2Encoding.EstimateMessageCount(70);
        Assert.Equal(1, count70);

        // Exactly 134 characters (67 * 2) should be 2 messages
        var count134 = Ucs2Encoding.EstimateMessageCount(134);
        Assert.Equal(2, count134);
    }
}
