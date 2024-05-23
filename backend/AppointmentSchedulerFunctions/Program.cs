using AppointmentSchedulerFunctions.Interfaces;
using AppointmentSchedulerFunctions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddTransient<ISmsGateway, SmsLinkGateway>();
        // services.AddTransient<ISmsGateway, ACSGateway>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
    })
    .Build();

host.Run();