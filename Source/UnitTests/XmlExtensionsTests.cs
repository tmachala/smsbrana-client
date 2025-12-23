using System.Globalization;
using System.Xml.Linq;
using SmsBranaClient.Formatting;
using Xunit;

namespace SmsBranaClient.UnitTests;

public class XmlExtensionsTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("0", 0)]
    [InlineData("-123", -123)]
    [InlineData("2147483647", 2147483647)] // int.MaxValue
    [InlineData("-2147483648", -2147483648)] // int.MinValue
    public void ReadRequiredInt_ReturnsCorrectValue(string value, int expected)
    {
        var xml = new XElement("root", new XElement("number", value));
        var result = xml.ReadRequiredInt("number");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadRequiredInt_ThrowsWhenElementMissing()
    {
        var xml = new XElement("root");
        var ex = Assert.Throws<InvalidOperationException>(() => xml.ReadRequiredInt("number"));
        Assert.Contains("number", ex.Message);
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void ReadRequiredInt_ThrowsWhenValueInvalid()
    {
        var xml = new XElement("root", new XElement("number", "not-a-number"));
        Assert.Throws<FormatException>(() => xml.ReadRequiredInt("number"));
    }

    [Theory]
    [InlineData("123456789", 123456789L)]
    [InlineData("0", 0L)]
    [InlineData("-987654321", -987654321L)]
    [InlineData("9223372036854775807", 9223372036854775807L)] // long.MaxValue
    [InlineData("-9223372036854775808", -9223372036854775808L)] // long.MinValue
    public void ReadRequiredLong_ReturnsCorrectValue(string value, long expected)
    {
        var xml = new XElement("root", new XElement("number", value));
        var result = xml.ReadRequiredLong("number");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadRequiredLong_ThrowsWhenElementMissing()
    {
        var xml = new XElement("root");
        var ex = Assert.Throws<InvalidOperationException>(() => xml.ReadRequiredLong("number"));
        Assert.Contains("number", ex.Message);
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void ReadRequiredLong_ThrowsWhenValueInvalid()
    {
        var xml = new XElement("root", new XElement("number", "invalid"));
        Assert.Throws<FormatException>(() => xml.ReadRequiredLong("number"));
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("0", 0)]
    [InlineData("-99.99", -99.99)]
    [InlineData("0.01", 0.01)]
    [InlineData("1000000.50", 1000000.50)]
    public void ReadRequiredDecimal_ReturnsCorrectValue(string value, double expectedDouble)
    {
        var expected = (decimal)expectedDouble;
        var xml = new XElement("root", new XElement("price", value));
        var result = xml.ReadRequiredDecimal("price");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadRequiredDecimal_UsesInvariantCulture()
    {
        // Decimal with dot separator (invariant culture)
        var xml = new XElement("root", new XElement("price", "123.45"));
        var result = xml.ReadRequiredDecimal("price");
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ReadRequiredDecimal_ThrowsWhenElementMissing()
    {
        var xml = new XElement("root");
        var ex = Assert.Throws<InvalidOperationException>(() => xml.ReadRequiredDecimal("price"));
        Assert.Contains("price", ex.Message);
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void ReadRequiredDecimal_ThrowsWhenValueInvalid()
    {
        var xml = new XElement("root", new XElement("price", "not-a-decimal"));
        Assert.Throws<FormatException>(() => xml.ReadRequiredDecimal("price"));
    }

    [Theory]
    [InlineData("Hello World", "Hello World")]
    [InlineData("test", "test")]
    [InlineData("123", "123")]
    public void ReadOptionalString_ReturnsValue(string value, string expected)
    {
        var xml = new XElement("root", new XElement("text", value));
        var result = xml.ReadOptionalString("text");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadOptionalString_ReturnsNull_WhenElementMissing()
    {
        var xml = new XElement("root");
        var result = xml.ReadOptionalString("text");
        Assert.Null(result);
    }

    [Fact]
    public void ReadOptionalString_ReturnsNull_WhenValueEmpty()
    {
        var xml = new XElement("root", new XElement("text", ""));
        var result = xml.ReadOptionalString("text");
        Assert.Null(result);
    }

    [Fact]
    public void ReadOptionalString_PreservesWhitespace()
    {
        var xml = new XElement("root", new XElement("text", "  spaces  "));
        var result = xml.ReadOptionalString("text");
        Assert.Equal("  spaces  ", result);
    }

    [Fact]
    public void ReadRequiredInt_HandlesNestedElements()
    {
        var xml = new XElement("root", 
            new XElement("parent", 
                new XElement("child", "42")));
        var parent = xml.Element("parent");
        var result = parent!.ReadRequiredInt("child");
        Assert.Equal(42, result);
    }

    [Fact]
    public void ReadOptionalString_HandlesMultipleElements()
    {
        var xml = new XElement("root",
            new XElement("first", "value1"),
            new XElement("second", "value2"));
        
        Assert.Equal("value1", xml.ReadOptionalString("first"));
        Assert.Equal("value2", xml.ReadOptionalString("second"));
        Assert.Null(xml.ReadOptionalString("third"));
    }
}
