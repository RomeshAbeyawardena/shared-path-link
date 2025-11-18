using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Tests;

public record Customer : MappableBase<ICustomer>, ICustomer
{
    protected override ICustomer Source => this;
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }

    public override void Map(ICustomer source)
    {
        Id = source.Id;
        Name = source.Name;
        Address = source.Address;
        City = source.City;
        Region = source.Region;
        PostalCode = source.PostalCode;
    }
}
