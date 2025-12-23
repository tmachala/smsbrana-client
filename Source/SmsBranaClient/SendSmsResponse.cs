namespace SmsBranaClient;

public class SendSmsResponse
{
    /// <summary>
    /// The price of a single message part in CZK including VAT.
    /// </summary>
    public required decimal Price { get; set; }
    
    /// <summary>
    /// The total number of messages billed (long messages get split in multiple parts).
    /// </summary>
    public required int SmsCount { get; set; }

    /// <summary>
    /// The remaining credit balance in CZK including VAT.
    /// </summary>
    public required decimal Credit { get; set; }

    /// <summary>
    /// The ID assigned by the service. Can be used to track the delivery status of the SMS.
    /// The documentation doesn't say whether it's int or long so we treat is as long.
    /// </summary>
    public required long SmsId { get; set; }

    /// <summary>
    /// An optional client-provided message ID.
    /// </summary>
    public required string? UserId { get; set; }
}