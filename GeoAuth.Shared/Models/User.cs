using GeoAuth.Shared.Extensions;

namespace GeoAuth.Shared.Models;

public class UserDto : MappableBase<IUser>, IUser
{
    protected override IUser Source => this;
    public string? Id { get; set; }
    Guid? IUser.Id => this.GetGuid(Id);
    Guid? IUser.ClientId => this.GetGuid(ClientId);
    public string? ClientId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Secret { get; set; }
    public string? Salt { get; set; }

    public override void Map(IUser source)
    {
        Id = source.Id.ToString();
        ClientId = source.ClientId.ToString();
        Email = source.Email;
        Name = source.Name;
        Secret = source.Secret;
        Salt = source.Salt;
    }
}

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
