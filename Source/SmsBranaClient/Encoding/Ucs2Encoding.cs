namespace SmsBranaClient.Encoding;

internal static class Ucs2Encoding
{
    /// <summary>
    /// Gets the message length in characters (as an old cell phone would count it when typing).
    /// </summary>
    public static int GetMessageLength(string text)
    {
        // UCS2 is basically UTF-16 without surrogates, so it's always 2 bytes per character.
        return text.ReplaceLineEndings("\n").Length;
    }

    public static int EstimateMessageCount(int messageLength)
    {
        // UCS2 messages are limited to 70 characters.
        if (messageLength <= 70) 
            return 1;

        // Multipart UCS2 messages can contain 67 characters each.
        return (int)Math.Ceiling(messageLength / 67d);
    }
}
