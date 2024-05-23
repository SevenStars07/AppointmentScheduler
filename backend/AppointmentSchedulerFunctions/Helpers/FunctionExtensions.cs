using System.Net;
using AppointmentSchedulerFunctions.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace AppointmentSchedulerFunctions.Helpers;

public static class FunctionExtensions
{
    public static async Task<HttpResponseData> CreateOKResponseAsJson(this HttpRequestData request,
        Appointment appointment)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(appointment);
        return response;
    }

    public static async Task<HttpResponseData> CreateBadRequestResponseAsJson(this HttpRequestData request,
        List<string> errors)
    {
        var response = request.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(errors);
        return response;
    }
}