using AppointmentSchedulerFunctions.Interfaces;
using Azure.Communication.Sms;
using Microsoft.Extensions.Logging;

namespace AppointmentSchedulerFunctions.Services;

public class ACSGateway(ILoggerFactory loggerFactory): ISmsGateway
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SmsLinkGateway>();
    public Task SendSms(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureCommunicationServicesConnectionString");
        var fromPhoneNumber = Environment.GetEnvironmentVariable("AzureCommunicationServicesFromPhoneNumber");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(fromPhoneNumber))
        {
            _logger.LogError("Azure Communication Services connection string or from phone number is not set.");
            return Task.CompletedTask;
        }

        var smsClient = new SmsClient(connectionString);
        try
        {
            SmsSendResult sendResult = smsClient.Send(
                from: fromPhoneNumber,
                to: phoneNumber,
                message: message,
                cancellationToken: cancellationToken
            );
            _logger.LogInformation($"Notification sent successfully. Response: {sendResult.MessageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send notification. Error: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }
}