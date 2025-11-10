using Microsoft.AspNetCore.Http;
using System.Collections;

namespace geo_auth.Models;

internal class ResponseException: Exception
{
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
