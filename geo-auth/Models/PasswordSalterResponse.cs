using System.Text.Json.Serialization;

namespace geo_auth.Models;

internal interface IPasswordSalterResponse
{
    string? Hash { get; }
    string? Salt { get; }
}

internal record PasswordSalterResponse : StandardResponse<IPasswordSalterResponse, PasswordSalterResponse>, IPasswordSalterResponse
{
    protected override PasswordSalterResponse? Result => this;
    public PasswordSalterResponse()
    {
        
    }

    public PasswordSalterResponse(Guid? automationId) : base(automationId)
    {
        
    }

    [JsonIgnore]
    public string? Hash { get; init; }

    [JsonIgnore]
    public string? Salt { get; init; }
}
