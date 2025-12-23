using SmsBranaClient.Formatting;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("123456789", "123456789")] // No formatting
    [InlineData("123 456 789", "123456789")] // Spaces
    [InlineData("(123) 456-789", "123456789")] // Parentheses and hyphens
    [InlineData("123-456-789", "123456789")] // Hyphens only
    [InlineData("(123)456789", "123456789")] // Parentheses only
    [InlineData("+420 123 456 789", "+420123456789")] // International format with spaces
    [InlineData("+1 (555) 123-4567", "+15551234567")] // US format
    [InlineData("(+420) 123-456-789", "+420123456789")] // Mixed formatting
    public void NormalizeOrNull_RemovesFormatting(string input, string? expected)
    {
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void NormalizeOrNull_ReturnsNull_ForEmptyOrWhitespace(string? input)
    {
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeOrNull_PreservesDigitsAndPlus()
    {
        var input = "+420123456789";
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal("+420123456789", result);
    }

    [Fact]
    public void NormalizeOrNull_HandlesComplexFormatting()
    {
        var input = "(123) 456 - 789";
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal("123456789", result);
    }

    [Fact]
    public void NormalizeOrNull_PreservesOtherCharacters()
    {
        // The method only removes spaces, parentheses, and hyphens
        var input = "+420#123*456";
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal("+420#123*456", result);
    }

    [Fact]
    public void NormalizeOrNull_HandlesMultipleSpaces()
    {
        var input = "123  456   789";
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal("123456789", result);
    }

    [Fact]
    public void NormalizeOrNull_HandlesMultipleParentheses()
    {
        var input = "((123))456789";
        var result = PhoneNumber.NormalizeOrNull(input);
        Assert.Equal("123456789", result);
    }
}
