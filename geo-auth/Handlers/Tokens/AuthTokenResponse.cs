using geo_auth.Models;
using System.Text.Json.Serialization;

namespace geo_auth.Handlers.Tokens;

public record AuthTokenResult(string? Token) : IAuthTokenResult
{
}

internal record AuthTokenResponse : MappableStandardResponse<IAuthTokenResult, AuthTokenResponse>, IAuthTokenResult
{
    protected override IAuthTokenResult Source => this;
    protected override AuthTokenResponse? Result => this;

    public AuthTokenResponse(IAuthTokenResult response, Guid? automationId)
        : base(response, automationId)
    {

    }

    [JsonIgnore]
    public string? Token { get; private set; }

    public override void Map(IAuthTokenResult source)
    {
        Token = source.Token;
    }
}
