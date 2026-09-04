using FluentValidation;
using ITI.ERP.Application.DTOs.Batch;

namespace ITI.ERP.Application.Validators.Batch;

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Batch name is required.")
            .MaximumLength(200).WithMessage("Batch name must not exceed 200 characters.");

        RuleFor(x => x.TradeId)
            .NotEmpty().WithMessage("Trade ID is required.");

        RuleFor(x => x.StartAcademicSessionId)
            .NotEmpty().WithMessage("Start Academic Session ID is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Batch start date is required.");

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Batch code must not exceed 20 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).When(x => x.Capacity.HasValue)
            .WithMessage("Capacity must be greater than 0.");
    }
}

public class UpdateBatchRequestValidator : AbstractValidator<UpdateBatchRequest>
{
    public UpdateBatchRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Batch name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Batch code must not exceed 20 characters.")
            .When(x => x.Code is not null);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).When(x => x.Capacity.HasValue)
            .WithMessage("Capacity must be greater than 0.");
    }
}
