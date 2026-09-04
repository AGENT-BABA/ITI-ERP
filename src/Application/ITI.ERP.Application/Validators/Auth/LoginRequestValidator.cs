using FluentValidation;
using ITI.ERP.Application.DTOs.Auth;

namespace ITI.ERP.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.GRNumber)
            .NotEmpty()
            .Length(1, 10);

        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(1, 50);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(6, 100);
    }
}
