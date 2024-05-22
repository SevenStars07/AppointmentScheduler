using Newtonsoft.Json;

namespace AppointmentSchedulerFunctions.Models;

public class Appointment
{
    public string id => AppointmentId.ToString();
    
    [JsonIgnore]
    public Guid AppointmentId { get; init; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string PhoneNumber { get; set; }
}