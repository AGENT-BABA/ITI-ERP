using FluentValidation;
using ITI.ERP.Application.DTOs.User;

namespace ITI.ERP.Application.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(1, 100)
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => x.LastName is not null);

        RuleFor(x => x.Phone)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
