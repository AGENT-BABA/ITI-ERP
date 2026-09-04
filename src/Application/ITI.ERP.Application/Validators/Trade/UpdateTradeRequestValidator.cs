using FluentValidation;
using ITI.ERP.Application.DTOs.Trade;

namespace ITI.ERP.Application.Validators.Trade;

public class UpdateTradeRequestValidator : AbstractValidator<UpdateTradeRequest>
{
    public UpdateTradeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(1, 20)
            .When(x => x.Code is not null);

        RuleFor(x => x.DurationInMonths)
            .InclusiveBetween(1, 48)
            .When(x => x.DurationInMonths.HasValue);

        RuleFor(x => x.TotalSeats)
            .InclusiveBetween(1, 1000)
            .When(x => x.TotalSeats.HasValue);
    }
}
