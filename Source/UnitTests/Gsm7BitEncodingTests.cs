using SmsBranaClient.Encoding;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class Gsm7BitEncodingTests
{
    [Theory]
    [InlineData("Hello World", true)]
    [InlineData("1234567890", true)]
    [InlineData("@£$¥èéùìòÇ", true)] // Basic set
    [InlineData("^{}\\[]~|€", true)] // Extended set
    [InlineData("Hello € World", true)] // Mixed
    [InlineData("abcdefghijklmnopqrstuvwxyz", true)]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", true)]
    [InlineData("", true)] // Empty string
    [InlineData("Hello\nWorld", true)] // Line feed
    [InlineData("ØøÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ", true)] // More basic set chars
    public void IsGsm7_ReturnsTrue_ForValidCharacters(string input, bool expected)
    {
        var result = Gsm7BitEncoding.IsGsm7(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("ž", false)]
    [InlineData("😊", false)]
    [InlineData("š", false)]
    [InlineData("č", false)]
    [InlineData("ř", false)]
    [InlineData("©", false)] // Not in GSM 7-bit
    [InlineData("Hello ž World", false)] // Mixed with invalid
    public void IsGsm7_ReturnsFalse_ForInvalidCharacters(string input, bool expected)
    {
        var result = Gsm7BitEncoding.IsGsm7(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello\r\nWorld", true)] // Windows line ending (gets normalized to \n)
    [InlineData("Hello\rWorld", true)] // Old Mac line ending (gets normalized to \n)
    public void IsGsm7_HandlesLineEndings(string input, bool expected)
    {
        var result = Gsm7BitEncoding.IsGsm7(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello", 5)]
    [InlineData("€", 2)] // Extended char is 2 bytes
    [InlineData("He€lo", 6)] // 4 basic + 1 extended (2) = 6
    [InlineData("", 0)]
    [InlineData("[]", 4)] // Two extended chars = 4
    [InlineData("^{}\\", 8)] // Four extended chars = 8
    [InlineData("Hello\n", 6)] // Newline is a basic char
    public void GetMessageLength_CalculatesCorrectly(string input, int expectedLength)
    {
        var length = Gsm7BitEncoding.GetMessageLength(input);
        Assert.Equal(expectedLength, length);
    }

    [Fact]
    public void GetMessageLength_ThrowsArgumentException_ForInvalidCharacter()
    {
        Assert.Throws<ArgumentException>(() => Gsm7BitEncoding.GetMessageLength("Hello ž"));
    }

    [Theory]
    [InlineData("😊")]
    [InlineData("č")]
    [InlineData("©")]
    public void GetMessageLength_ThrowsArgumentException_ForVariousInvalidCharacters(string input)
    {
        var ex = Assert.Throws<ArgumentException>(() => Gsm7BitEncoding.GetMessageLength(input));
        Assert.Contains("Invalid character", ex.Message);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 1)]
    [InlineData(160, 1)]
    [InlineData(161, 2)]
    [InlineData(306, 2)] // 153 * 2
    [InlineData(307, 3)]
    [InlineData(459, 3)] // 153 * 3
    [InlineData(460, 4)]
    [InlineData(1, 1)]
    [InlineData(153, 1)]
    public void EstimateMessageCount_CalculatesCorrectly(int length, int expectedCount)
    {
        var count = Gsm7BitEncoding.EstimateMessageCount(length);
        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData("Hello World", "Hello World")] // No conversion needed
    [InlineData("Čau", "Cau")] // Diacritic removal
    [InlineData("šťřž", "strz")] // Multiple diacritics
    [InlineData("–", "-")] // En dash replacement
    [InlineData("—", "-")] // Em dash replacement
    [InlineData("©", "?")] // Fallback
    [InlineData("", "")] // Empty string
    [InlineData("€100", "€100")] // Valid extended char preserved
    [InlineData("naïve café", "naive café")] // 'é' is valid GSM 7-bit, only 'ï' is converted
    public void Convert_ReturnsExpectedString(string input, string expected)
    {
        var result = Gsm7BitEncoding.Convert(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_HandlesSmartQuotes()
    {
        // Test smart quotes separately to avoid escaping issues in InlineData
        Assert.Equal("\"Hello\"", Gsm7BitEncoding.Convert("\u201cHello\u201d"));
        Assert.Equal("'Hello'", Gsm7BitEncoding.Convert("\u2018Hello\u2019"));
    }

    [Theory]
    [InlineData("–", "-", true)] // En dash with replacement enabled
    [InlineData("–", "?", false)] // En dash without replacement (falls back to ?)
    public void Convert_RespectsReplaceSimilarCharsParameter(string input, string expected, bool replaceSimilarChars)
    {
        var result = Gsm7BitEncoding.Convert(input, replaceSimilarChars: replaceSimilarChars);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_RespectsReplaceSimilarCharsParameter_SmartQuotes()
    {
        // Test smart quotes with parameter
        Assert.Equal("\"", Gsm7BitEncoding.Convert("\u201c", replaceSimilarChars: true));
        Assert.Equal("?", Gsm7BitEncoding.Convert("\u201c", replaceSimilarChars: false));
    }

    [Fact]
    public void Convert_PreservesValidGsm7Characters()
    {
        var input = "Hello World! @£$¥€";
        var result = Gsm7BitEncoding.Convert(input);
        Assert.Equal(input, result);
    }

    [Fact]
    public void Convert_HandlesComplexMixedContent()
    {
        var input = "Čau! Café costs €5 – awesome";
        var expected = "Cau! Café costs €5 - awesome"; // é is valid GSM 7-bit
        var result = Gsm7BitEncoding.Convert(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("é", "é", EAcuteHandling.Preserve)]
    [InlineData("é", "e", EAcuteHandling.Strip)]
    [InlineData("Café", "Café", EAcuteHandling.Preserve)]
    [InlineData("Café", "Cafe", EAcuteHandling.Strip)]
    [InlineData("été", "été", EAcuteHandling.Preserve)]
    [InlineData("été", "ete", EAcuteHandling.Strip)]
    public void Convert_EAcuteHandling_WorksCorrectly(string input, string expected, EAcuteHandling handling)
    {
        var result = Gsm7BitEncoding.Convert(input, eAcuteHandling: handling);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_EAcuteHandling_Preserve_IsDefault()
    {
        // When not specified, é should be preserved
        var result = Gsm7BitEncoding.Convert("Café");
        Assert.Equal("Café", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_Strip_ConvertsAllInstances()
    {
        // Use chars not in GSM 7-bit: š, ř, ů
        var input = "réšumé of Petr's škola";
        var result = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Strip);
        
        // All lowercase é should be converted to e, other accented chars removed
        Assert.DoesNotContain("é", result);
        Assert.Equal("resume of Petr's skola", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_Strip_OnlyAffectsLowercaseE()
    {
        // É (uppercase) is also in GSM 7-bit, but should not be affected by strip
        var input = "É and é";
        var result = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Strip);
        
        Assert.Equal("É and e", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_WorksWithOtherConversions()
    {
        // Test that é handling works alongside diacritic removal and similar char replacement
        var input = "Café – šéé";
        var result = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Strip, replaceSimilarChars: true);
        
        Assert.Equal("Cafe - see", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_Strip_WithDiacriticsDisabled()
    {
        var input = "Café – test";
        var result = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Strip, replaceSimilarChars: false);
        
        // é stripped, en-dash becomes ? (replaceSimilarChars is false)
        Assert.Equal("Cafe ? test", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_EmptyString()
    {
        var result = Gsm7BitEncoding.Convert("", eAcuteHandling: EAcuteHandling.Strip);
        Assert.Equal("", result);
    }

    [Fact]
    public void Convert_EAcuteHandling_NoEInString()
    {
        var input = "Hello World";
        
        var preserve = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Preserve);
        var strip = Gsm7BitEncoding.Convert(input, eAcuteHandling: EAcuteHandling.Strip);
        
        // Should be identical when there's no é
        Assert.Equal(input, preserve);
        Assert.Equal(input, strip);
    }
}

