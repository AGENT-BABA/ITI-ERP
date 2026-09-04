using FluentValidation;
using ITI.ERP.Application.DTOs.Student;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.Validators.Student;

public class ChangeStudentStatusRequestValidator : AbstractValidator<ChangeStudentStatusRequest>
{
    public ChangeStudentStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.NewStatus is StudentStatus.Inactive or StudentStatus.Transferred
                or StudentStatus.Withdrawn or StudentStatus.CancelledAdmission);
    }
}
