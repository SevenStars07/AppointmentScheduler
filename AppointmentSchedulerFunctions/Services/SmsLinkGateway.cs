using AppointmentSchedulerFunctions.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppointmentSchedulerFunctions.Services;

public class SmsLinkGateway(ILoggerFactory loggerFactory): ISmsGateway
{
    private static readonly HttpClient HttpClient = new();
    private readonly ILogger _logger = loggerFactory.CreateLogger<SmsLinkGateway>();
    
    public async Task SendSms(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        var uri = Environment.GetEnvironmentVariable("SmsLink:Uri");

        var connectionId = Environment.GetEnvironmentVariable("SmsLink:ConnectionId");
        var password = Environment.GetEnvironmentVariable("SmsLink:Password");
        
        if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(connectionId) || string.IsNullOrEmpty(password))
        {
            _logger.LogError("SMS configuration is missing.");
            return;
        }
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["connection_id"] = connectionId,
            ["password"] = password,
            ["to"] = phoneNumber,
            ["message"] = message
        });
        
        var response = await HttpClient.PostAsync(new Uri(uri), content, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"Notification sent successfully. Response: {contentString}");
        }
        else
        {
            _logger.LogError("Failed to send notification.");
        }
    }
}