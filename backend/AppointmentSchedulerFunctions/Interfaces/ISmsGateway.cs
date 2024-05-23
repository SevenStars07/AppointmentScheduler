namespace AppointmentSchedulerFunctions.Interfaces;

public interface ISmsGateway
{
    Task SendSms(string phoneNumber, string message, CancellationToken cancellationToken);
}