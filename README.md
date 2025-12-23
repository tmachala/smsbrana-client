# SmsBranaClient

A .NET library for sending SMS messages via smsbrana.cz.

This is a 3rd party library Not affiliated with smsbrana.cz, the service provider.

## Compatibility

* .NET 10
* Any platform supported by .NET
* Any OS supported by .NET

## Installation

```
dotnet add package SmsBranaClient
```

## Usage

### Configuration

Configure the credentials. The snippet below shows the expected structure. Don't put the password in `appsettings.json`. Treat it as a secret.

```json
{
  "SmsBrana": {
    "Login": "your_login",
    "Password": "your_password"
  }
}
```

### Registration

Register the client in your `Program.cs`. The usual patterns apply:

* **Option 1:**

  ```csharp
  // Defaults to reading from the "SmsBrana" section but can be overriden.
  builder.Services.AddSmsBrana(builder.Configuration);
  ```

* **Option 2:**
  ```csharp
  builder.Services.AddSmsBrana(options =>
  {
      options.Username = "your_login";
      options.Password = "your_password";
  });
  ```

* **Option 3:**
  ```csharp
  builder.Services.Configure<SmsBranaOptions>(config.GetSection("SmsBrana"));
  builder.Services.AddSmsBrana();
  ```

### Sending SMS

```csharp
var requestBuilder = new SendSmsRequestBuilder("Příliš žluťoučký kůň úpěl ďábelské ódy", "+420123456789")
{
    // A sane default: use Unicode but only when it doesn't cost extra
    AllowUnicode = AllowUnicode.OnlyWhenSamePrice,
    
    // Works well for the Czech language (see below).
    EAcuteHandling = EAcuteHandling.Strip,
    
    // We could set other options here...
};

var request = requestBuilder.Build();

try
{
    var result = await client.SendSmsAsync(request);

    if (result.Success)
    {
        var response = result.Response!;
        Console.WriteLine("SMS sent successfully.");
        Console.WriteLine("SMS ID: {0}", response.SmsId);
        Console.WriteLine("Parts:  {0}", response.SmsCount);
        Console.WriteLine("Price:  {0}", response.Price);
    }
    else
    {
        Console.WriteLine("Error sending SMS!");
        Console.WriteLine("Error code: " + result.ErrorCode);
        Console.WriteLine("Error message: " + result.ErrorMessage);
    }
}
catch (Exception e)
{
    Console.WriteLine("Failed to send SMS: " + e.Message);
    throw;
}
```

### Service Lifetime

`SmsBranaClient` is registered as a **Transient** service. If you need to use if inside a singleton service, you can do so in a custom scope:

```csharp
public class MySingletonService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MySingletonService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task DoSomethingAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ISmsBranaClient>();
        
        // Use the client
        // ...
    }
}
```

## Features

### Cost-Effective Encoding

There are two encodings available for SMS:

* **GSM 7-bit**: The legacy standard. Allows for 160 characters.
* **UCS 2**: An Unicode encoding. Only allows for 70 characters since each character is 2 bytes.

| Encoding  | Max Length | Max Length of Multipart Messages |
| --------- | ---------- | -------------------------------- |
| GSM 7-bit | 160        | 153                              |
| UCS 2     | 70         | 67                               |

Note that with GSM 7-bit, some characters count as two: `^{}\\[]~|€` and the form feed (`\f`).

By default, the library will use UCS 2 but only when it doesn't cost extra. If the message is longer than 70 characters (and would therefore be split into multiple parts, each costing money), the library will convert it to GSM 7-bit encoding automatically, losing some characters in the process.

The behavior is controlled by the `AllowUnicode` property of the `SendSmsRequestBuilder`:

| Value                         | Description                                                                                                                             |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `Never`                       | Never use Unicode.                                                                                                                      |
| `AlwaysWhenNeeded`            | Use Unicode unless all characters are representable by GSM 7-bit without loss of information.                                           |
| `OnlyWhenSamePrice` (default) | Use Unicode only when the message fits into one part. Otherwise, convert to GSM 7-bit to reduce the message count (and thus the price). |

### Smart Character Conversion

When converting messages from UCS 2 to GSM 7-bit, the library will try to preserve the original characters as much as possible. For example, it will convert various proper parentheses `„“”` to the standard `"`. Same with dashes: `—` and `–` are converted to `-`.

### é/É Handling

In the Czech language in particular, there are multiple characters unrepresentable by the GSM 7-bit encoding: `ěščřžýáíúů` and maybe some others. However, there is also `é`, which **is** representable. That's a problem because when you remove all diacricics except for `é`/`É`, then the result looks weird. It's better to either keep all diacritics (when using UCS 2) or remove it completely (GSM 7-bit). Consider this example:

* `Příliš žluťoučký kůň úpěl ďábelské ódy` (original)
* `Prilis zlutoucky kun upel dabelské ody` (GSM 7-bit; ugly because only `é` is preserved)
* `Prilis zlutoucky kun upel dabelske ody` (GSM 7-bit; all diacritics removed → somewhat prettier)

So, as a rule of thumb, when sending a message in Czech, always set `EAcuteHandling` to `Strip`. It is ignored when the message ends being sent in UCS 2 and so there should really be no harm in doing so.

