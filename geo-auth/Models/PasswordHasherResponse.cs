using GeoAuth.Shared.Requests.Passwords;

namespace geo_auth.Models;

internal record PasswordHasherResponse : StandardResponse<IPasswordHash, PasswordHash>
{
    protected override PasswordHash? Result { get; }
    public PasswordHasherResponse(PasswordHash result)
    {
        Result = result;
    }

    public PasswordHasherResponse(PasswordHash result, Guid? automationId) : base(automationId)
    {
        Result = result;
    }
}
