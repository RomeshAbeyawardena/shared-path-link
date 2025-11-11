using Microsoft.AspNetCore.Http;
using System.Collections;

namespace geo_auth.Models;

internal class ResponseException: Exception
{
    /// <summary>
    /// Wraps an exception into <see cref="ResponseException"/> if the passed <paramref name="exception"/> is not a derived <see cref="ResponseException"/>.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="wrapperTransformedStatusCode">
    ///     If the exception is not a <see cref="ResponseException"/> this status code will be persisted in the returned <see cref="ResponseException"/>
    /// </param>
    /// <returns></returns>
    public static ResponseException Transform(Exception exception, int? wrapperTransformedStatusCode = null)
    {
        if (exception is ResponseException responseException)
        {
            return responseException;
        }

        return new ResponseException(exception, wrapperTransformedStatusCode.GetValueOrDefault(StatusCodes.Status500InternalServerError));
    }

    public int? StatusCode { get; set; }
    public ResponseException(string message, int? statusCode, Exception? innerException = null)
        : this(new Exception(message, innerException), statusCode.GetValueOrDefault(StatusCodes.Status500InternalServerError))
    {

    }

    public ResponseException(Exception exception, int statusCode)
       : base(exception.Message, exception.InnerException)
    {
        HelpLink = exception.HelpLink;
        StatusCode = statusCode;
        StackTrace = exception.StackTrace;
        Data = exception.Data;
    }

    public override string? StackTrace { get; }
    public override IDictionary Data { get; }
}
