namespace ProfidLauncherUpdater.Infrastructure;

public class Result<T>
{
    public bool IsSuccess { get; set; }

    public bool IsFailure => !IsSuccess;

    public readonly T Value = default!;

    public Error Error { get; }

    private Result(T v, bool isSuccess, Error error)
    {
        Value = v;

        if (isSuccess && error != Error.None
            || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T v) => new(v, true, Error.None);

    public static Result<T> Failure(Error error) => new(default(T)!, false, error);

    public static implicit operator Result<T>(T v) => new(v, true, Error.None);
    public static implicit operator Result<T>(Error e) => new(default(T)!, false, e);

    public R Match<R>(
        Func<T, R> success,
        Func<Error, R> failure) =>
           IsSuccess ? success(Value) : failure(Error);
}
