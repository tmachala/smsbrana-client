namespace SmsBranaClient;

public class SendSmsRequest
{
    public required string Number { get; init; }
    public required string Message { get; init; }
    public required DateTime? When { get; init; }
    public required bool DeliveryReport { get; init; }
    public required string? SenderId { get; init; }
    public required string? SenderPhone { get; init; }
    public required string? CustomMessageId { get; init; }
    public required SmsBranaEncoding Encoding { get; init; }
    public required string? AnswerMail { get; init; }
    public required string? DeliveryMail { get; init; }
}