using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Extensions;

public static class ResultExtensions
{
    public static void EnsureSuccessOrThrow<T>(this IResult<T> result)
    {
        if (!result.IsSuccess)
        {
            if (result.Exception is not null)
            {
                throw ResponseException.Transform(result.Exception);
            }

            throw new ResponseException("An unexpected error occurred", 500);
        }
    }
}
