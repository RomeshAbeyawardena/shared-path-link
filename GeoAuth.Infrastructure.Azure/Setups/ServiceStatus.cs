namespace GeoAuth.Infrastructure.Azure.Setups;

public record ServiceStatus(string Key, Type Type, bool? Exists, Exception? Exception = null)
{
    public bool IsError => Exception is not null;
}
