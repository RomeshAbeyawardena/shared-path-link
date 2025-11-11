using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Requests.Passwords
{
    public interface IPasswordHash : IMappable<IPasswordHash>
    {
        string Hash { get; }
        string? Salt { get; }
    }
}
