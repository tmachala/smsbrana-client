using System.Globalization;
using System.Xml.Linq;

namespace SmsBranaClient.Formatting;

public static class XmlExtensions
{
    public static int ReadRequiredInt(this XElement node, string name)
    {
        var s = node.Element(name)?.Value ?? throw new InvalidOperationException($"The required element {name} is missing in the XML response!");
        return int.Parse(s, CultureInfo.InvariantCulture);
    }
    
    public static long ReadRequiredLong(this XElement node, string name)
    {
        var s = node.Element(name)?.Value ?? throw new InvalidOperationException($"The required element {name} is missing in the XML response!");
        return long.Parse(s, CultureInfo.InvariantCulture);
    }
    
    public static decimal ReadRequiredDecimal(this XElement node, string name)
    {
        var s = node.Element(name)?.Value ?? throw new InvalidOperationException($"The required element {name} is missing in the XML response!");
        return decimal.Parse(s, CultureInfo.InvariantCulture);
    }
    
    public static string? ReadOptionalString(this XElement node, string name)
    {
        var s = node.Element(name)?.Value;
        return !string.IsNullOrEmpty(s) ? s : null;
    }
}