using FluentValidation;
using ITI.ERP.Application.DTOs.Auth;

namespace ITI.ERP.Application.Validators.Auth;

public class VerifyResetTokenRequestValidator : AbstractValidator<VerifyResetTokenRequest>
{
    public VerifyResetTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
