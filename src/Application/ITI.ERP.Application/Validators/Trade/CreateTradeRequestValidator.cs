using FluentValidation;
using ITI.ERP.Application.DTOs.Trade;

namespace ITI.ERP.Application.Validators.Trade;

public class CreateTradeRequestValidator : AbstractValidator<CreateTradeRequest>
{
    public CreateTradeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(1, 20);

        RuleFor(x => x.DurationInMonths)
            .InclusiveBetween(1, 48);

        RuleFor(x => x.TotalSeats)
            .InclusiveBetween(1, 1000);
    }
}
