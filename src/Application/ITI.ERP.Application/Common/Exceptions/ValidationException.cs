using FluentValidation.Results;

namespace ITI.ERP.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationResult ValidationResult { get; }

    public ValidationException(ValidationResult validationResult)
        : base("One or more validation failures have occurred.")
    {
        ValidationResult = validationResult;
    }
}
