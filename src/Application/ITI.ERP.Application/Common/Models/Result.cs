namespace ITI.ERP.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string>? Errors { get; }

    private Result()
    {
        IsSuccess = true;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    public static Result Success() => new();
    public static Result Failure(string error) => new(error);
    public static Result Failure(List<string> errors) => new(errors);
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public List<string>? Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }

    private Result(List<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(List<string> errors) => new(errors);
}
