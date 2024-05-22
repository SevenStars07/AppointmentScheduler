using System.Globalization;
using AppointmentSchedulerFunctions.Interfaces;
using AppointmentSchedulerFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace AppointmentSchedulerFunctions.Functions;

public class AppointmentCreatedSequence(ISmsGateway smsGateway, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AppointmentCreatedSequence>();

    [Function(nameof(AppointmentCreated))]
    public async Task AppointmentCreated([CosmosDBTrigger(
            databaseName: "appointments",
            containerName: "appointments",
            Connection = "CosmosDbConnectionSetting",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Appointment> appointments,
        [DurableClient] DurableTaskClient client,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        if (appointments is not { Count: > 0 }) return;


        var instanceId =
            await client.ScheduleNewOrchestrationInstanceAsync(nameof(NotifyUsersOrchestrator), appointments,
                cancellationToken);
        _logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);
    }

    [Function(nameof(NotifyUsersOrchestrator))]
    public async Task NotifyUsersOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var appointments = context.GetInput<IReadOnlyList<Appointment>>();

        if (appointments is not { Count: > 0 }) return;

        foreach (var appointment in appointments)
        {
            await context.CallSubOrchestratorAsync(nameof(NotifyUserSubOrchestrator), appointment);
        }
    }

    [Function(nameof(NotifyUserSubOrchestrator))]
    public async Task NotifyUserSubOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context, Appointment appointment,
        CancellationToken cancellationToken)
    {
        if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development")
        {
            await context.CreateTimer(TimeSpan.FromSeconds(5), cancellationToken);
        }
        else
        {
            await context.CreateTimer(TimeZoneInfo.ConvertTimeToUtc(appointment.Date).AddHours(-24), cancellationToken);
        }

        await context.CallActivityAsync(nameof(NotifyUser), appointment);
    }

    [Function(nameof(NotifyUser))]
    public async Task NotifyUser([ActivityTrigger] Appointment appointment, FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Sending notification for appointment {appointment.AppointmentId} to {appointment.Name}");

        var date = appointment.Date
            .ToString("dddd, d MMMM yyyy 'ora' HH:mm", CultureInfo.CreateSpecificCulture("ro-RO"));

        var message = $"Programarea dvs. pentru data de {date} a fost inregistrata cu succes.";
        
        _logger.LogInformation(message);

        // await smsGateway.SendSms(appointment.PhoneNumber, message, cancellationToken);
    }
}