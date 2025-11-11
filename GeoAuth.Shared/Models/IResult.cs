namespace GeoAuth.Shared.Models;

public interface IResult<T>
{
    Exception? Exception { get; }
    T? Result { get; }
    bool IsSuccess { get; }
}
