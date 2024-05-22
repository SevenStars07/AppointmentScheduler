using System.Text.Json;
using AppointmentSchedulerFunctions.Helpers;
using AppointmentSchedulerFunctions.Inputs;
using AppointmentSchedulerFunctions.Models;
using AppointmentSchedulerFunctions.Outputs;
using AppointmentSchedulerFunctions.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AppointmentSchedulerFunctions.Functions;

public class CreateAppointment(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CreateAppointment>();

    [Function("CreateAppointment")]
    public async Task<AppointmentsMultiResponse> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "create-appointment")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("Create appointment function triggered.");

        var createAppointmentInput = await req.Body.Deserialize<CreateAppointmentInput>();

        var validator = new CreateAppointmentInputValidator();

        var validationResult = await validator.ValidateAsync(createAppointmentInput);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning($"Create appointment validation failed. {string.Join(", ", errors)}");

            return new AppointmentsMultiResponse
            {
                HttpResponse = await req.CreateBadRequestResponseAsJson(errors),
            };
        }

        var appointment = createAppointmentInput.ToAppointment();

        var appointmentJson =
            JsonSerializer.Serialize(new AppointmentCreatedMessage { AppointmentId = appointment.AppointmentId });

        _logger.LogInformation("Appointment processing finished successfully.");

        return new AppointmentsMultiResponse
        {
            HttpResponse = await req.CreateOKResponseAsJson(appointment),
            QueueMessages = [appointmentJson],
            Appointment = appointment
        };
    }
}