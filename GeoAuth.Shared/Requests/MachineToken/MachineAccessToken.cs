using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Shared.Requests.MachineToken
{
    public record MachineAccessToken : MappableBase<IMachineAccessToken>, IMachineAccessToken
    {
        protected override IMachineAccessToken Source => this;
        public string? Token { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset Expires { get; set; }
        public Guid MachineId { get; set; }
        public Guid Id { get; set; }

        public override void Map(IMachineAccessToken source)
        {
            Token = source.Token;
            ValidFrom = source.ValidFrom;
            Expires = source.Expires;
            MachineId = source.MachineId;
            Id = source.Id;
        }
    }
}
