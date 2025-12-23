using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmsBranaClient.Cli;

public class Program
{
    private static async Task Main(string[] args)
    {
        // ####################################################################
        // WARNING: Sending SMSes costs real money! Make sure not to introduce
        // any unwanted loop in your code. Also, don't let your AI agent loose.
        // ####################################################################
        
        var builder = Host.CreateApplicationBuilder(args);
        var services = builder.Services;
        
        var config = builder.Configuration;
        
        config.AddUserSecrets<Program>();
        
        services.AddSmsBrana(config);

        var app = builder.Build();
        
        var client = app.Services.GetRequiredService<ISmsBranaClient>();

        var username = config.GetValue<string>("SmsBrana:Username");
        var password = config.GetValue<string>("SmsBrana:Password");
        var myPhoneNumber = config.GetValue<string>("MyPhoneNumber");
        
        var requiredParams = new[] {username, password, myPhoneNumber};
        if (requiredParams.Any(string.IsNullOrEmpty))
        {
            Console.WriteLine("You need to configure the project first:");
            Console.WriteLine("  dotnet user-secrets set \"SmsBrana:Username\" \"...\"");
            Console.WriteLine("  dotnet user-secrets set \"SmsBrana:Password\" \"...\"");
            Console.WriteLine("  dotnet user-secrets set \"MyPhoneNumber\" \"+420...\"");
            return;
        }
        
        var smsBuilder = new SendSmsRequestBuilder("Příliš žluťoučký kůň úpěl ďábelské ódy", myPhoneNumber!)
        {
            // A sane default: use Unicode but only when it doesn't cost extra
            AllowUnicode = AllowUnicode.OnlyWhenSamePrice,
            
            // Works well for the Czech language (see README.md).
            EAcuteHandling = EAcuteHandling.Strip,
            
            // We could set other options here...
        };
        
        var request = smsBuilder.Build();

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
                Console.WriteLine("Error code:    " + result.ErrorCode);
                Console.WriteLine("Error message: " + result.ErrorMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception occurred while sending SMS: " + e.Message);
            throw;
        }
    }
}