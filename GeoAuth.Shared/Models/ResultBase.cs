namespace GeoAuth.Shared.Models;

public abstract record ResultBase<T>(T? Result, Exception? Exception = null) : IResult<T>
{
    public bool IsSuccess => Exception is null && Result is not null;
}
