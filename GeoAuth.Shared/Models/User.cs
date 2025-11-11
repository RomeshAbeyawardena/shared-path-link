namespace GeoAuth.Shared.Models;

public class User : IUser
{
    public Guid? Id { get; set; }
    public Guid? ClientId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Secret { get; set; }
    public string? Salt { get; set; }
}
