using FluentValidation;
using ITI.ERP.Application.DTOs.YearlyPractical;

namespace ITI.ERP.Application.Validators.YearlyPractical;

public class CreateYearlyPracticalRequestValidator : AbstractValidator<CreateYearlyPracticalRequest>
{
    public CreateYearlyPracticalRequestValidator()
    {
        RuleFor(x => x.TradeId)
            .NotEmpty();

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class SubmitYearlyPracticalMarksRequestValidator : AbstractValidator<SubmitYearlyPracticalMarksRequest>
{
    public SubmitYearlyPracticalMarksRequestValidator()
    {
        RuleFor(x => x.YearlyPracticalId)
            .NotEmpty();
    }
}
