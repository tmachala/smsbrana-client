using System;

namespace SmsBranaClient;

public class SmsBranaClientOptions
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public Uri Uri { get; set; } = new("https://api.smsbrana.cz/smsconnect/http.php");
}
