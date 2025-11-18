using GeoAuth.Shared.Models;

namespace GeoAuth.Tests;

public interface ICustomer : IMappable<ICustomer>
{
    Guid Id { get;}
    string? Name { get;}
    string? Address { get;}
    string? City { get; }
    string? Region { get;}
    string? PostalCode { get; }
}
