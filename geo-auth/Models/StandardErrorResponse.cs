namespace geo_auth.Models;

internal abstract record StandardErrorResponse : StandardResponse<StandardErrorResponse>
{
    protected StandardErrorResponse(Exception exception, int statusCode)
    {
        Message = exception.Message;
        Details = exception.StackTrace;
        StatusCode = statusCode;
    }
}
