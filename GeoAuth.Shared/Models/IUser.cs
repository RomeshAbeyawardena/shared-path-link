namespace GeoAuth.Shared.Models;

public interface IUser : IMappable<IUser>
{
    Guid? Id { get; }
    Guid? ClientId { get; }
    string? Email { get; }
    string? Name { get; }
    string? Secret { get; }
    string? Salt { get; }
}
