using Microsoft.AspNetCore.Http;

namespace geo_auth.Models;

internal record StandardErrorResponse : StandardResponse<StandardErrorResponse>
{
    protected override StandardErrorResponse? Result => this;

    public StandardErrorResponse(string message, int? statusCode, Exception? innerException = null)
        : this(new StandardErrorResponse(message, statusCode, innerException))
    {

    }

    public StandardErrorResponse(Exception exception, int statusCode)
    {
        Message = exception.Message;
        Details = exception.StackTrace;
        StatusCode = statusCode;
    }

    public StandardErrorResponse(ResponseException responseException)
        : this(responseException, responseException.StatusCode.GetValueOrDefault(StatusCodes.Status500InternalServerError))
    {

    }
}
