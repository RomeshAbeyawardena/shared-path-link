using GeoAuth.Shared.Exceptions;

namespace GeoAuth.Shared.Models;

public abstract record ResultBase<T>(T? Result, Exception? Exception = null) : IResult<T>
{
    public bool IsSuccess => Exception is null && Result is not null;
}

public static class Result
{
    public static IResult<T> Sucessful<T>(T value) => new GenericResult<T>(value);
    public static IResult<T> Failed<T>(Exception exception) => new GenericResult<T>(default, exception);
    public static IResult<T> FailedResponse<T>(Exception exception, int? statusCode = null) 
        => Failed<T>(new ResponseException(exception, statusCode.GetValueOrDefault(500)));
}

internal record GenericResult<T>(T? Result, Exception? Exception = null) : ResultBase<T>(Result, Exception)
{

}