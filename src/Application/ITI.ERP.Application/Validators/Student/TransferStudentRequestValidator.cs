using FluentValidation;
using ITI.ERP.Application.DTOs.Student;

namespace ITI.ERP.Application.Validators.Student;

public class TransferStudentRequestValidator : AbstractValidator<TransferStudentRequest>
{
    public TransferStudentRequestValidator()
    {
        RuleFor(x => x.NewTradeId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
