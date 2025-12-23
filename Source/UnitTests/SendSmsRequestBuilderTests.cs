using Xunit;

namespace SmsBranaClient.UnitTests;

public class SendSmsRequestBuilderTests
{
    [Fact]
    public void Constructor_DefaultValues()
    {
        var builder = new SendSmsRequestBuilder();
        
        Assert.Equal("", builder.Message);
        Assert.Equal("", builder.PhoneNumber);
        Assert.Equal(AllowUnicode.OnlyWhenSamePrice, builder.AllowUnicode);
        Assert.Equal(EAcuteHandling.Preserve, builder.EAcuteHandling);
        Assert.Null(builder.WhenUtc);
        Assert.False(builder.DeliveryReport);
        Assert.Null(builder.SenderId);
        Assert.Null(builder.SenderPhone);
        Assert.Null(builder.CustomMessageId);
        Assert.Null(builder.AnswerMail);
        Assert.Null(builder.DeliveryMail);
    }

    [Fact]
    public void Constructor_WithParameters()
    {
        var builder = new SendSmsRequestBuilder("Hello", "+420123456789", AllowUnicode.Never);
        
        Assert.Equal("Hello", builder.Message);
        Assert.Equal("+420123456789", builder.PhoneNumber);
        Assert.Equal(AllowUnicode.Never, builder.AllowUnicode);
    }

    [Fact]
    public void Build_ThrowsWhenMessageEmpty()
    {
        var builder = new SendSmsRequestBuilder { PhoneNumber = "123" };
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void Build_ThrowsWhenPhoneNumberEmpty()
    {
        var builder = new SendSmsRequestBuilder { Message = "Hello" };
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void Build_NormalizesPhoneNumber()
    {
        var builder = new SendSmsRequestBuilder("Hello", "(123) 456-789");
        var request = builder.Build();
        
        Assert.Equal("123456789", request.Number);
    }

    [Fact]
    public void Build_TrimsAndNormalizesMessage()
    {
        var builder = new SendSmsRequestBuilder("  Hello\r\nWorld  ", "123");
        var request = builder.Build();
        
        Assert.Equal("Hello\nWorld", request.Message);
    }

    [Fact]
    public void Build_UsesGsm7ForGsmMessages()
    {
        var builder = new SendSmsRequestBuilder("Hello World", "123");
        var request = builder.Build();
        
        Assert.Equal("Hello World", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }

    [Fact]
    public void Build_UsesUcs2ForUnicodeInSinglePart()
    {
        var builder = new SendSmsRequestBuilder("Hello ž", "123");
        var request = builder.Build();
        
        Assert.Equal("Hello ž", request.Message);
        Assert.Equal(SmsBranaEncoding.Ucs2, request.Encoding);
    }

    [Fact]
    public void Build_ConvertsToGsm7ForMultipartUnicode_OnlyWhenSamePrice()
    {
        // Create a message that exceeds 70 UCS2 chars (single part limit)
        var longMessage = new string('x', 65) + "ž" + new string('y', 10); // 76 chars with UCS2
        var builder = new SendSmsRequestBuilder(longMessage, "123", AllowUnicode.OnlyWhenSamePrice);
        var request = builder.Build();
        
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
        Assert.DoesNotContain("ž", request.Message);
        Assert.Contains("z", request.Message); // Converted
    }

    [Fact]
    public void Build_PreservesUcs2ForMultipart_WhenNeededEvenIfMoreExpensive()
    {
        var longMessage = new string('x', 65) + "ž" + new string('y', 10);
        var builder = new SendSmsRequestBuilder(longMessage, "123", AllowUnicode.AlwaysWhenNeeded);
        var request = builder.Build();
        
        Assert.Equal(SmsBranaEncoding.Ucs2, request.Encoding);
        Assert.Contains("ž", request.Message);
    }

    [Fact]
    public void Build_ForcesGsm7_Never()
    {
        var builder = new SendSmsRequestBuilder("Hello ž", "123", AllowUnicode.Never);
        var request = builder.Build();
        
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
        Assert.DoesNotContain("ž", request.Message);
    }

    [Fact]
    public void Build_SetsWhenToPragueTime()
    {
        var utc = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var builder = new SendSmsRequestBuilder("Hello", "123") { WhenUtc = utc };
        var request = builder.Build();
        
        Assert.NotNull(request.When);
        Assert.Equal(13, request.When.Value.Hour); // UTC+1 in winter
    }

    [Fact]
    public void Build_SetsDeliveryReport()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { DeliveryReport = true };
        var request = builder.Build();
        
        Assert.True(request.DeliveryReport);
    }

    [Fact]
    public void Build_SetsSenderId()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { SenderId = "MySender" };
        var request = builder.Build();
        
        Assert.Equal("MySender", request.SenderId);
    }

    [Fact]
    public void Build_NormalizesSenderPhone()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { SenderPhone = "(123) 456-789" };
        var request = builder.Build();
        
        Assert.Equal("123456789", request.SenderPhone);
    }

    [Fact]
    public void Build_SetsCustomMessageId()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { CustomMessageId = "my-id-123" };
        var request = builder.Build();
        
        Assert.Equal("my-id-123", request.CustomMessageId);
    }

    [Fact]
    public void Build_ThrowsWhenCustomMessageIdTooLong()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") 
        { 
            CustomMessageId = new string('x', 51) // Max is 50
        };
        
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_SetsAnswerMail()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { AnswerMail = "answer@example.com" };
        var request = builder.Build();
        
        Assert.Equal("answer@example.com", request.AnswerMail);
    }

    [Fact]
    public void Build_SetsDeliveryMail()
    {
        var builder = new SendSmsRequestBuilder("Hello", "123") { DeliveryMail = "delivery@example.com" };
        var request = builder.Build();
        
        Assert.Equal("delivery@example.com", request.DeliveryMail);
    }

    [Fact]
    public void Build_ThrowsWhenMessageTooLong()
    {
        var builder = new SendSmsRequestBuilder(new string('x', 460), "123"); // Max is 459
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ThrowsWhenPhoneNumberNormalizedToNull()
    {
        var builder = new SendSmsRequestBuilder("Hello", "   "); // Only whitespace
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Phone number", ex.Message);
    }

    [Fact]
    public void Build_HandlesComplexScenario()
    {
        var utc = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var builder = new SendSmsRequestBuilder
        {
            Message = "  Hello\r\nWorld  ",
            PhoneNumber = "(+420) 123-456-789",
            AllowUnicode = AllowUnicode.OnlyWhenSamePrice,
            WhenUtc = utc,
            DeliveryReport = true,
            SenderId = "Company",
            SenderPhone = "987-654-321",
            CustomMessageId = "msg-001",
            AnswerMail = "answer@company.com",
            DeliveryMail = "delivery@company.com"
        };

        var request = builder.Build();

        Assert.Equal("Hello\nWorld", request.Message);
        Assert.Equal("+420123456789", request.Number);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
        Assert.NotNull(request.When);
        Assert.Equal(12, request.When.Value.Hour); // UTC+2 in summer
        Assert.True(request.DeliveryReport);
        Assert.Equal("Company", request.SenderId);
        Assert.Equal("987654321", request.SenderPhone);
        Assert.Equal("msg-001", request.CustomMessageId);
        Assert.Equal("answer@company.com", request.AnswerMail);
        Assert.Equal("delivery@company.com", request.DeliveryMail);
    }

    [Fact]
    public void Build_EAcuteHandling_DefaultIsPreserve()
    {
        var builder = new SendSmsRequestBuilder("Café", "123");
        var request = builder.Build();
        
        // Default should preserve é
        Assert.Equal("Café", request.Message);
    }

    [Fact]
    public void Build_EAcuteHandling_Preserve_KeepsE()
    {
        var builder = new SendSmsRequestBuilder("Café", "123") 
        { 
            EAcuteHandling = EAcuteHandling.Preserve 
        };
        var request = builder.Build();
        
        Assert.Equal("Café", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }

    [Fact]
    public void Build_EAcuteHandling_Strip_RemovesE()
    {
        var builder = new SendSmsRequestBuilder("Café", "123") 
        { 
            EAcuteHandling = EAcuteHandling.Strip 
        };
        var request = builder.Build();
        
        Assert.Equal("Cafe", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }

    [Fact]
    public void Build_EAcuteHandling_Strip_WorksWithForceGsm7()
    {
        // With unicode chars that would normally trigger UCS2
        var builder = new SendSmsRequestBuilder("Café with škorice", "123") 
        { 
            AllowUnicode = AllowUnicode.Never,
            EAcuteHandling = EAcuteHandling.Strip 
        };
        var request = builder.Build();
        
        Assert.DoesNotContain("é", request.Message);
        Assert.Equal("Cafe with skorice", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }

    [Fact]
    public void Build_EAcuteHandling_IgnoredForUcs2()
    {
        // Short message with UCS2 chars should preserve é even with Strip
        var builder = new SendSmsRequestBuilder("Café š", "123") 
        { 
            EAcuteHandling = EAcuteHandling.Strip 
        };
        var request = builder.Build();
        
        // UCS2 encoding is used, so é is preserved
        Assert.Equal("Café š", request.Message);
        Assert.Equal(SmsBranaEncoding.Ucs2, request.Encoding);
    }

    [Fact]
    public void Build_EAcuteHandling_Strip_WorksWithMultipartConversion()
    {
        // Long message with UCS2 that gets converted to GSM7
        var longMessage = new string('x', 65) + "café" + new string('y', 10);
        var builder = new SendSmsRequestBuilder(longMessage, "123") 
        { 
            AllowUnicode = AllowUnicode.OnlyWhenSamePrice,
            EAcuteHandling = EAcuteHandling.Strip 
        };
        var request = builder.Build();
        
        Assert.DoesNotContain("é", request.Message);
        Assert.Contains("cafe", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }

    [Fact]
    public void Build_EAcuteHandling_ComplexScenario()
    {
        var builder = new SendSmsRequestBuilder
        {
            Message = "  réšumé  ",
            PhoneNumber = "123",
            AllowUnicode = AllowUnicode.Never,
            EAcuteHandling = EAcuteHandling.Strip
        };
        var request = builder.Build();
        
        // Message trimmed, é stripped, other diacritics removed
        Assert.Equal("resume", request.Message);
        Assert.Equal(SmsBranaEncoding.Gsm7Bit, request.Encoding);
    }
}

