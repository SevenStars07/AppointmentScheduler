using FluentValidation;

namespace AppointmentSchedulerFunctions;

public class CreateAppointmentInputValidator : AbstractValidator<CreateAppointmentInput>
{
    public CreateAppointmentInputValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("The date is required")
            .GreaterThan(DateTime.Now)
            .WithMessage("The appointment date must be in the future");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("The name is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("The phone number is required");
    }
}