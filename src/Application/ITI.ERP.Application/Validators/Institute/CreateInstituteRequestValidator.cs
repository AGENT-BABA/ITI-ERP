using FluentValidation;
using ITI.ERP.Application.DTOs.Institute;

namespace ITI.ERP.Application.Validators.Institute;

public class CreateInstituteRequestValidator : AbstractValidator<CreateInstituteRequest>
{
    public CreateInstituteRequestValidator()
    {
        RuleFor(x => x.GRNumber)
            .NotEmpty()
            .Length(1, 10)
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("GR Number must be alphanumeric.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\+\-\(\)\s]{7,15}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format.");
    }
}
