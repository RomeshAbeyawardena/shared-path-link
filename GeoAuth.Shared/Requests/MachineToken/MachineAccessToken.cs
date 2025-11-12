using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Shared.Requests.MachineToken
{
    public record MachineAccessToken : MappableBase<IMachineAccessToken>, IMachineAccessToken
    {
        protected override IMachineAccessToken Source => this;
        public string? Token { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset Expires { get; set; }
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public override void Map(IMachineAccessToken source)
        {
            Token = source.Token;
            ValidFrom = source.ValidFrom;
            Expires = source.Expires;
            PartitionKey = source.PartitionKey;
            RowKey = source.RowKey;
            Timestamp = source.Timestamp;
        }
    }
}
