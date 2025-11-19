using System.Diagnostics.CodeAnalysis;

namespace GeoAuth.Shared.Models;

public interface IResult<T>
{
    Exception? Exception { get; }
    T? Result { get; }

    [MemberNotNull(nameof(Result))]
    bool IsSuccess { get; }
}
