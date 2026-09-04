namespace ITI.ERP.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You are not authorized to perform this action.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
