using GeoAuth.Shared.Exceptions;
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
        : this(exception, statusCode, null)
    {
        
    }

    public StandardErrorResponse(Exception exception, int statusCode, Guid? automationId)
        : base(automationId)
    {
        Message = exception.Message;
        Details = exception.StackTrace ?? exception.InnerException?.Message;
        StatusCode = statusCode;
    }

    public StandardErrorResponse(ResponseException responseException, Guid? automationId)
        : this(responseException, responseException.StatusCode.GetValueOrDefault(StatusCodes.Status500InternalServerError), automationId)
    {

    }
}
