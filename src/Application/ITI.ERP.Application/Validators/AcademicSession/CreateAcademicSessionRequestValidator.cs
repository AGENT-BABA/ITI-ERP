using FluentValidation;
using ITI.ERP.Application.DTOs.AcademicSession;

namespace ITI.ERP.Application.Validators.AcademicSession;

public class CreateAcademicSessionRequestValidator : AbstractValidator<CreateAcademicSessionRequest>
{
    public CreateAcademicSessionRequestValidator()
    {
        RuleFor(x => x.SessionYear)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}$")
            .WithMessage("Session Year must be in 'YYYY-YY' format.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .LessThan(x => x.EndDate)
            .WithMessage("Start Date must be before End Date.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End Date must be after Start Date.");
    }
}
