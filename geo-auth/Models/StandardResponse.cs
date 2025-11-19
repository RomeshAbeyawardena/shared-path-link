using GeoAuth.Shared.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace geo_auth.Models;

internal abstract record MappableStandardResponse<TContract, T> : StandardResponse<TContract, T>, IMappable<TContract>
    where T : TContract
{
    protected abstract TContract Source { get; }

    protected MappableStandardResponse(TContract contract, Guid? automationId)
        : base(automationId)
    {
        Map(contract);
    }

    public TResult Map<TResult>(Func<TResult> instanceFactory) where TResult : IMappable<TContract>
    {
        var result = instanceFactory();
        result.Map(Source);
        return result;
    }

    public TResult Map<TResult>() where TResult : IMappable<TContract>, new()
    {
        return Map(() => new TResult());
    }

    public abstract void Map(TContract source);
}


internal abstract record StandardResponse<TContract, T> : StandardResponse<T>
    where T : TContract
{
    protected StandardResponse(Guid? automationId)
    {
        AutomationId = automationId;
    }

    protected StandardResponse() : this((Guid?)null)
    {

    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public TContract? Data => Result;
}

internal abstract record StandardResponse<T> : IResult
{
    protected StandardResponse(Guid? automationId)
    {
        AutomationId = automationId;
    }

    protected StandardResponse() : this((Guid?)null)
    {
        
    }

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
