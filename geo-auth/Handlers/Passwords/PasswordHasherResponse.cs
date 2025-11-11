using geo_auth.Models;
using GeoAuth.Shared.Requests.Passwords;
using System.Text.Json.Serialization;

namespace geo_auth.Handlers.Passwords;

internal record PasswordHashResponse : MappableStandardResponse<IPasswordHash, PasswordHashResponse>, IPasswordHash
{
    protected override IPasswordHash Source => this;
    protected override PasswordHashResponse? Result => this;

    public PasswordHashResponse(IPasswordHash passwordHash, Guid? automationId) 
        : base(passwordHash, automationId)
    {
    }

    [JsonIgnore]
    public string Hash { get; private set; } = null!;
    
    [JsonIgnore]
    public string? Salt { get; private set; }

    public override void Map(IPasswordHash source)
    {
        Hash = source.Hash;
        Salt = source.Salt;
    }

}
