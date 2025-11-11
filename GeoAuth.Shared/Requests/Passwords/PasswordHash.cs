using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Requests.Passwords
{
    public class PasswordHash : MappableBase<IPasswordHash>, IPasswordHash
    {
        protected override IPasswordHash Source => this;
        public string Hash { get; set; } = default!;
        public string? Salt { get; set; }
        public override void Map(IPasswordHash source)
        {
            Hash = source.Hash;
            Salt = source.Salt;
        }
    }
}
