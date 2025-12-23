# SmsBranaClient

A .NET library for sending SMS messages via smsbrana.cz.

This is a 3rd party library Not affiliated with smsbrana.cz.

## Compatibility

* .NET 10
* Any platform supported by .NET
* Any OS supported by .NET

## Installation

```
dotnet add package SmsBranaClient
```

## Usage

### 1. Configuration & Registration

Register the client in your `Program.cs` (or `Startup.cs`). The client is registered as a **Transient** service.

#### Option A: Using `appsettings.json` (Recommended)

Add the configuration section to your `appsettings.json`:

```json
{
  "SmsBrana": {
    "Login": "your_login",
    "Password": "your_password"
  }
}
```

Then register the client:

```csharp
builder.Services.AddSmsBrana(builder.Configuration);
// Defaults to reading from the "SmsBrana" section.
```

#### Option B: Manual Configuration

```csharp
builder.Services.AddSmsBrana(options =>
{
    options.Login = "your_login";
    options.Password = "your_password";
});
```

### 2. Sending an SMS

Inject `ISmsBranaClient` into your service or controller:

```csharp
public class NotificationService
{
    private readonly ISmsBranaClient _smsClient;

    public NotificationService(ISmsBranaClient smsClient)
    {
        _smsClient = smsClient;
    }

    public async Task SendNotificationAsync(string phoneNumber)
    {
        var result = await _smsClient.SendSmsAsync(phoneNumber, "Hello from .NET!");

        if (result.Success)
        {
            Console.WriteLine("SMS sent successfully.");
        }
        else
        {
            Console.WriteLine($"Error: {result.ErrorCode} - {result.ErrorMessage}");
        }
    }
}
```