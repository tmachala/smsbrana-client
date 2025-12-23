namespace SmsBranaClient;

public enum AllowUnicode
{
    /// <summary>
    /// Forces the message to be sent as GSM 7-bit.
    /// </summary>
    Never,
    
    /// <summary>
    /// Preserves UCS2 encoding unless the message only contains GSM 7-bit characters.
    /// </summary>
    AlwaysWhenNeeded,
    
    /// <summary>
    /// Preserves UCS2 encoding only when the message fits into one part. Multipart messages are sent as GSM 7-bit,
    /// making the operation cheaper.
    /// </summary>
    OnlyWhenSamePrice
}