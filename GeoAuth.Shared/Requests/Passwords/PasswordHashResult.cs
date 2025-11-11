using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Requests.Passwords
{
    public record PasswordHashResult(PasswordHash Result, Exception? Exception) : ResultBase<PasswordHash>(Result, Exception)
    {

    }
}
