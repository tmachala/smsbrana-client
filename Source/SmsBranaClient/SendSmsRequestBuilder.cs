using SmsBranaClient.Encoding;
using SmsBranaClient.Formatting;

namespace SmsBranaClient;

public class SendSmsRequestBuilder
{
    public string Message { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public AllowUnicode AllowUnicode { get; set; } = AllowUnicode.OnlyWhenSamePrice;
    public DateTime? WhenUtc { get; set; }
    public bool DeliveryReport { get; set; } = false;
    public string? SenderId { get; set; }
    public string? SenderPhone { get; set; }
    public string? CustomMessageId { get; set; }
    public string? AnswerMail { get; set; }
    public string? DeliveryMail { get; set; }
    
    /// <summary>
    /// Gets or sets the handling of the 'é' character in case the message is encoded using the GSM 7-bit encoding.
    /// </summary>
    /// <remarks>
    /// The Czech language in particular contains multiple characters such as ěščřžýíů unsupported by GSM 7-bit.
    /// The 'é' in question technically is supported. However, when we strip most characters of their accent and keep
    /// 'é' as the only exception, it looks worse visually than converting it to 'e'.
    /// Setting this flag to <see cref="EAcuteHandling.Strip"/> enforces such conversion. Ignored when UCS2 encoding
    /// is used since in that case, all Czech characters are supported.
    /// </remarks>
    public EAcuteHandling EAcuteHandling { get; set; } = EAcuteHandling.Preserve;
    
    public SendSmsRequestBuilder()
    { }

    public SendSmsRequestBuilder(string message, string phoneNumber, AllowUnicode allowUnicode = AllowUnicode.OnlyWhenSamePrice)
    {
        Message = message;
        PhoneNumber = phoneNumber;
        AllowUnicode = allowUnicode;
    }

    public SendSmsRequest Build()
    {
        ArgumentException.ThrowIfNullOrEmpty(Message, nameof(Message));
        ArgumentException.ThrowIfNullOrEmpty(PhoneNumber, nameof(PhoneNumber));

        var (message, encoding) = OptimizePrice();
        var phoneNumber = Formatting.PhoneNumber.NormalizeOrNull(PhoneNumber);
        var when = WhenUtc?.UtcToPragueTime();

        if (message.Length > MaxMessageLength)
        {
            throw new InvalidOperationException($"Message cannot be longer than {MaxMessageLength} characters!");
        }
        
        if (phoneNumber == null)
        {
            throw new InvalidOperationException($"Phone number cannot be empty!");
        }

        if (CustomMessageId?.Length > CustomMessageIdMaxLength)
        {
            throw new InvalidOperationException($"Custom message ID cannot be longer than {CustomMessageIdMaxLength} characters!");
        }

        return new SendSmsRequest
        {
            Message = message,
            Number = phoneNumber,
            Encoding = encoding,
            When = when,
            DeliveryReport = DeliveryReport,
            SenderId = SenderId,
            SenderPhone = Formatting.PhoneNumber.NormalizeOrNull(SenderPhone),
            CustomMessageId = CustomMessageId,
            AnswerMail = AnswerMail,
            DeliveryMail = DeliveryMail
        };
    }

    private (string message, SmsBranaEncoding encoding) OptimizePrice()
    {
        var normalizedMessage = Message.ReplaceLineEndings("\n").Trim();
        
        // When the message contains nothing but GSM 7-bit characters or when we want to force GSM 7-bit
        if (Gsm7BitEncoding.IsGsm7(normalizedMessage) || AllowUnicode == AllowUnicode.Never)
        {
            return (Gsm7BitEncoding.Convert(normalizedMessage, EAcuteHandling), SmsBranaEncoding.Gsm7Bit);   
        }
        
        var partCount = Ucs2Encoding.EstimateMessageCount(Ucs2Encoding.GetMessageLength(normalizedMessage));

        // When the message fits into one part despite having non-GSM 7bit characters, there is nothing more to do
        if (partCount == 1)
        {
            return (normalizedMessage, SmsBranaEncoding.Ucs2);
        }
        
        // If there are more parts, and we prefer price to readability, convert to GSM 7-bit
        if (AllowUnicode == AllowUnicode.OnlyWhenSamePrice)
        {
            return (Gsm7BitEncoding.Convert(normalizedMessage, EAcuteHandling), SmsBranaEncoding.Gsm7Bit);   
        }
        
        // If there are more parts, but we want to keep UCS2 despite the price increase
        if (AllowUnicode == AllowUnicode.AlwaysWhenNeeded)
        {
            return (normalizedMessage, SmsBranaEncoding.Ucs2);
        }
        
        throw new InvalidOperationException($"Unexpected unicode mode: {AllowUnicode}!");
    }

    // From the SMS Connect documentation; unverified. Does it apply to UCS2 as well?
    private const int MaxMessageLength = 459;

    private const int CustomMessageIdMaxLength = 50;
}