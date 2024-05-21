namespace AppointmentSchedulerFunctions;

public static class AppointmentExtensions
{
    public static Appointment ToAppointment(this CreateAppointmentInput input)
    {
        return new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            Name = input.Name,
            Date = input.Date,
            PhoneNumber = input.PhoneNumber
        };
    }
}