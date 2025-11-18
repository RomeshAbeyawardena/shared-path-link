using GeoAuth.Shared.Projectors;

namespace GeoAuth.Tests;


public class Customer
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
}

public class CustomerDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
}

public class DictionaryProjectorTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var dictionaryProjector = DictionaryProjector<Customer>.Serialise();
        var dict = dictionaryProjector(new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Susan Hall",
            Address = "22 Crossbridge Avenue",
            City = "Edinburgh",
            Region = "Scotland",
            PostalCode = "ES1 NHX"
        });

        var dictionaryProjector2 = DictionaryProjector<Customer>.Hydrator();

        var result = dictionaryProjector2(dict);
        Assert.Pass();
    }
}
