namespace SmsBranaClient;

public class SendSmsResult
{
    public bool Success { get; private set; }
    
    /// <summary>
    /// The result object. Available only when <see cref="Success"/> is true.
    /// </summary>
    public SendSmsResponse? Response { get; private set; }
    
    /// <summary>
    /// The numeric error code. Available only when <see cref="Success"/> is false.
    /// </summary>
    public int? ErrorCode { get; private set; }
    
    /// <summary>
    /// THe Czech error message. Available only when <see cref="Success"/> is false.
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    public static SendSmsResult FromSuccess(SendSmsResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        
        return new SendSmsResult
        {
            Success = true,
            Response = response,
            ErrorCode = null,
            ErrorMessage = null
        };
    }
    
    public static SendSmsResult FromError(int errorCode, string? errorMessage = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(errorCode, nameof(errorCode));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        return new SendSmsResult
        {
            Success = false,
            Response = null,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}
