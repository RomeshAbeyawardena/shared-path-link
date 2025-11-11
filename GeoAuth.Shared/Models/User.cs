namespace GeoAuth.Shared.Models;

public class User : MappableBase<IUser>, IUser
{
    protected override IUser Source => this;
    public Guid? Id { get; set; }
    public Guid? ClientId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Secret { get; set; }
    public string? Salt { get; set; }

    public override void Map(IUser source)
    {
        Id = source.Id;
        ClientId = source.ClientId;
        Email = source.Email;
        Name = source.Name;
        Secret = source.Secret;
        Salt = source.Salt;
    }
}
