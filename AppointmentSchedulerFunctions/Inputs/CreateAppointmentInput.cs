namespace AppointmentSchedulerFunctions.Inputs;

public class CreateAppointmentInput
{
    public string Name { get; set; }
    public DateTimeOffset Date { get; set; }
    public string PhoneNumber { get; set; }
}