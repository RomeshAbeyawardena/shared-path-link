using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace geo_auth.Models;

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
