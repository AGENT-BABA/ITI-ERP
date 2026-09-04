using FluentValidation;
using ITI.ERP.Application.DTOs.AcademicSession;

namespace ITI.ERP.Application.Validators.AcademicSession;

public class UpdateAcademicSessionRequestValidator : AbstractValidator<UpdateAcademicSessionRequest>
{
    public UpdateAcademicSessionRequestValidator()
    {
        RuleFor(x => x.SessionYear)
            .Matches(@"^\d{4}-\d{2}$")
            .WithMessage("Session Year must be in 'YYYY-YY' format.")
            .When(x => x.SessionYear is not null);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Start Date must be before End Date.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End Date must be after Start Date.");
    }
}
