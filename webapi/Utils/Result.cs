using System.Text.Json.Serialization;

namespace ApiSgc.Utils;

public class Result
{
    public bool Success { get; }
    public string Message { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool success, string message, IEnumerable<string>? errors = null)
    {
        Success = success;
        Message = message;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    public static Result Ok(string message = "") => new(true, message);
    public static Result Failure(string message, IEnumerable<string>? errors = null) => new(false, message, errors);
    public static Result Failure(string message, string error) => new(false, message, new[] { error });

    public static Result<T> Ok<T>(T data, string message = "") => Result<T>.Ok(data, message);
    public static Result<T> Failure<T>(string message, IEnumerable<string>? errors = null) => Result<T>.Failure(message, errors);
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, T? data, string message, IEnumerable<string>? errors = null) 
        : base(success, message, errors)
    {
        Data = data;
    }

    public static Result<T> Ok(T data, string message = "") => new(true, data, message);
    public new static Result<T> Failure(string message, IEnumerable<string>? errors = null) => new(false, default, message, errors);
    public new static Result<T> Failure(string message, string error) => new(false, default, message, new[] { error });
}
