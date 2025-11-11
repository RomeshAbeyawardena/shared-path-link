namespace GeoAuth.Shared.Models;

public interface IUser : IMappable<IUser>
{
    Guid? Id { get; set; }
    Guid? ClientId { get; set; }
    string? Email { get; set; }
    string? Name { get; set; }
    string? Secret { get; set; }
    string? Salt { get; set; }
}
