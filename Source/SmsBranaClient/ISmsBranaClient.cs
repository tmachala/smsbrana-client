using System.Threading.Tasks;

namespace SmsBranaClient;

public interface ISmsBranaClient
{
    Task<SendSmsResult> SendSmsAsync(SendSmsRequest request, CancellationToken cancellationToken = default);
}
