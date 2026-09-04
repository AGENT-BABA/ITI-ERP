using FluentValidation;
using ITI.ERP.Application.DTOs.User;

namespace ITI.ERP.Application.Validators.User;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Length(6, 100)
            .Equal(x => x.ConfirmNewPassword)
            .WithMessage("New Password and Confirm New Password must match.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty();
    }
}
