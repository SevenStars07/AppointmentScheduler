using AppointmentSchedulerFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AppointmentSchedulerFunctions.Outputs;

public class AppointmentsMultiResponse
{
    [QueueOutput("outqueue", Connection = "AzureWebJobsStorage")]
    public string[] QueueMessages { get; set; }

    [CosmosDBOutput("appointments", "appointments",
        Connection = "CosmosDbConnectionSetting", CreateIfNotExists = true, PartitionKey = "/id")]
    public Appointment Appointment { get; set; }

    public HttpResponseData HttpResponse { get; set; }
}