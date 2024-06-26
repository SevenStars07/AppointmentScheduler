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
            await context.CallActivityAsync(nameof(NotifyUserAppointmentConfirmation), appointment);
            await context.CallSubOrchestratorAsync(nameof(NotifyUserReminderSubOrchestrator), appointment);
        }
    }

    [Function(nameof(NotifyUserAppointmentConfirmation))]
    public async Task NotifyUserAppointmentConfirmation([ActivityTrigger] Appointment appointment,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Sending confirmation notification for appointment {appointment.AppointmentId} to {appointment.Name} {appointment.PhoneNumber}");

        var userFriendlyDate = (appointment.Date + appointment.Offset)
            .ToString("dddd, d MMMM yyyy 'ora' HH:mm", CultureInfo.CreateSpecificCulture("ro-RO"));

        var message = $"Programarea dvs. pentru data de {userFriendlyDate} a fost inregistrata cu succes.";

        _logger.LogInformation(message);

        await smsGateway.SendSms(appointment.PhoneNumber, message, cancellationToken);
    }

    [Function(nameof(NotifyUserReminderSubOrchestrator))]
    public async Task NotifyUserReminderSubOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context, Appointment appointment,
        CancellationToken cancellationToken)
    {
        var fireAt = appointment.Date.AddHours(-24);
        _logger.LogInformation($"Scheduling notification for {fireAt}");

        await context.CreateTimer(fireAt, cancellationToken);

        await context.CallActivityAsync(nameof(NotifyUserAppointmentReminder), appointment);
    }

    [Function(nameof(NotifyUserAppointmentReminder))]
    public async Task NotifyUserAppointmentReminder([ActivityTrigger] Appointment appointment,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Sending reminder notification for appointment {appointment.AppointmentId} to {appointment.Name} {appointment.PhoneNumber}");

        var userFriendlyDate = (appointment.Date + appointment.Offset)
            .ToString("dddd, d MMMM yyyy 'ora' HH:mm", CultureInfo.CreateSpecificCulture("ro-RO"));

        var message = $"Va reamintim ca programarea dvs. pentru data de {userFriendlyDate} este maine.";

        _logger.LogInformation(message);

        await smsGateway.SendSms(appointment.PhoneNumber, message, cancellationToken);
    }
}