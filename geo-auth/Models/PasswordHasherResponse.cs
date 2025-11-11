using GeoAuth.Shared.Requests.Passwords;
using System.Text.Json.Serialization;

namespace geo_auth.Models;

internal record PasswordHashResponse : MappableStandardResponse<IPasswordHash, PasswordHashResponse>, IPasswordHash
{
    protected override IPasswordHash Source => this;
    protected override PasswordHashResponse? Result => this;

    [JsonIgnore]
    public string Hash { get; set; } = null!;
    
    [JsonIgnore]
    public string? Salt { get; set; }

    public PasswordHashResponse(IPasswordHash passwordHash, Guid? automationId) : base(passwordHash, automationId)
    {
    }

    public override void Map(IPasswordHash source)
    {
        Hash = source.Hash;
        Salt = source.Salt;
    }

}
