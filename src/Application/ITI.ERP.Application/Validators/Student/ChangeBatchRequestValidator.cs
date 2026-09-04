using FluentValidation;
using ITI.ERP.Application.DTOs.Student;

namespace ITI.ERP.Application.Validators.Student;

public class ChangeBatchRequestValidator : AbstractValidator<ChangeBatchRequest>
{
    public ChangeBatchRequestValidator()
    {
        RuleFor(x => x.NewBatchId)
            .NotEmpty().WithMessage("Target batch ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for batch change is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
