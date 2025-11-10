using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Text.Json.Serialization;

namespace geo_auth.Models;

internal abstract class ResponseException: Exception
{
    public int? StatusCode { get; set; }
    protected ResponseException(Exception exception, int statusCode)
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

internal abstract record StandardResponse<T> : IResult
{
    protected abstract T? Result { get; }
    public int? StatusCode { get; protected set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; protected set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; protected set; }

    public Guid? ResponseId { get; protected set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public Guid? AutomationId { get; protected set; }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        StatusCode = StatusCode.GetValueOrDefault(200);
        httpContext.Response.StatusCode = StatusCode.Value;
        
        ResponseId = ResponseId.GetValueOrDefault(Guid.NewGuid());

        if (Result is null)
        {
            await httpContext.Response.WriteAsJsonAsync(this);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(Result);
        }

        await httpContext.Response.CompleteAsync();
    }
}
