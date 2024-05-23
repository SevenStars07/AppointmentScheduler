using AppointmentSchedulerFunctions.Inputs;
using FluentValidation;

namespace AppointmentSchedulerFunctions.Validators;

public class CreateAppointmentInputValidator : AbstractValidator<CreateAppointmentInput>
{
    public CreateAppointmentInputValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("The date is required")
            .GreaterThan(DateTime.UtcNow.AddDays(1))
            .WithMessage("The appointment date must be at least one day from now");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("The name is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("The phone number is required")
            .Matches(@"^(?<paren>\()?(?:(?:72|74|75|76|77|78)(?(paren)\))(?<first>\d)(?!\k<first>{6})\d{6}|(?:251|351)(?(paren)\))(?<first>\d)(?!\k<first>{5})\d{5})$")
            .WithMessage("The phone number must match the Romanian mobile phone number format. Example: 7XXXXXXXX");

        RuleFor(x => x.PhoneNumberPrefix)
            .NotEmpty()
            .WithMessage("The phone number prefix is required")
            .Equal("+40")
            .WithMessage("The phone number prefix must be +40. Only Romania is supported for now.");
    }
}