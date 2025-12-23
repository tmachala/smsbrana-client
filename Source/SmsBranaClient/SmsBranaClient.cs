using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SmsBranaClient.Formatting;

namespace SmsBranaClient;

public class SmsBranaClient : ISmsBranaClient
{
    private readonly SmsBranaClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly TimeProvider _time;

    public SmsBranaClient(IOptions<SmsBranaClientOptions> options, HttpClient httpClient, TimeProvider time)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _time = time;
    }

    public async Task<SendSmsResult> SendSmsAsync(SendSmsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(request.Number, nameof(request.Number));
        ArgumentException.ThrowIfNullOrEmpty(request.Message, nameof(request.Message));
        
        var queryParams = BuildQueryParams(request);
        var finalUri = QueryHelpers.AddQueryString(_options.Uri.ToString(), queryParams);
        
        var response = await _httpClient.GetAsync(finalUri, cancellationToken);

        // The service returns HTTP 200 even on application-level errors. This deals with technical errors only.
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = doc.Root ?? throw new HttpRequestException("Malformed XML response!");

        // <result>
        if (!int.TryParse(root.Element("err")?.Value, out var errorCode))
        {
            throw new HttpRequestException("Malformed XML response!");
        }
        
        // If there was an application level error
        if (errorCode != 0)
        {
            var errorMessage = GetErrorMessage(errorCode);
            return SendSmsResult.FromError(errorCode, errorMessage);
        }

        var res = new SendSmsResponse
        {
            Credit = root.ReadRequiredDecimal("credit"),
            Price = root.ReadRequiredDecimal("price"),
            SmsCount = root.ReadRequiredInt("sms_count"),
            UserId = root.ReadOptionalString("user_id"),
            SmsId = root.ReadRequiredLong("sms_count")
        };
        
        return SendSmsResult.FromSuccess(res);
    }

    private Dictionary<string, string?> BuildQueryParams(SendSmsRequest request)
    {
        var (time, salt, auth) = ComputeAuthParams(_options.Password);

        var query = new Dictionary<string, string?>
        {
            ["login"] = _options.Username,
            ["time"] = time,
            ["sul"] = salt,
            ["auth"] = auth,
            ["action"] = "send_sms",
            ["number"] = request.Number,
            ["message"] = request.Message,
            ["delivery_report"] = request.DeliveryReport.ToQueryParam(),
            ["data_code"] = request.Encoding.ToQueryParam()
        };
        
        // Add optional params if present
        var when = request.When?.ToQueryParam();
        var senderId = request.SenderId;
        var senderPhone = request.SenderPhone;
        var userId = request.CustomMessageId;
        
        if (when != null)
            query.Add("when", when);
        
        if (!string.IsNullOrEmpty(senderId))
            query.Add("sender_id", senderId);
        
        if (!string.IsNullOrEmpty(senderPhone))
            query.Add("sender_phone", senderPhone);
        
        if (!string.IsNullOrEmpty(userId))
            query.Add("user_id", userId);
        
        return query;
    }

    private (string time, string salt, string auth) ComputeAuthParams(string password)
    {
        var dateTime = _time.GetUtcNow().DateTime.UtcToPragueTime();
        var time = dateTime.ToQueryParam();
        var salt = Guid.NewGuid().ToString("N"); // Max 50 chars
        
        var rawHash = Md5(password + time + salt);
        
        return (time, salt, rawHash);
    }
    
    private static string Md5(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string GetErrorMessage(int errorCode)
    {
        return errorCode switch
        {
            -1 => "duplicitní user_id",
            1 => "neznámá chyba",
            2 => "neplatný login",
            3 => "neplatný hash nebo password",
            4 => "neplatný time",
            5 => "nepovolená IP",
            6 => "neplatný název akce",
            7 => "tato sul byla již jednou za daný den použita",
            8 => "nebylo navázáno spojení s databází",
            9 => "nedostatečný kredit",
            10 => "neplatné číslo příjemce SMS",
            11 => "prázdný text zprávy",
            12 => "SMS je delší než povolených 459 znaků",
            _ => "Unknown error code"
        };
    }
}
