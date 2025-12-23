namespace SmsBranaClient;

/// <summary>
/// How do handle the 'é' character when using the GSM 7-bit encoding.
/// </summary>
public enum EAcuteHandling
{
    /// <summary>
    /// Keep 'é' as is.
    /// </summary>
    Preserve,
    
    /// <summary>
    /// Convert 'é' to 'e'.
    /// </summary>
    Strip
}