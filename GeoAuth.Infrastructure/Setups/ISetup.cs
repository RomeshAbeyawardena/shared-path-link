using GeoAuth.Infrastructure.Models;

namespace GeoAuth.Infrastructure.Setups;

public interface ISetup
{
    Task RunOnceAsync();
}


public interface IHealthCheckSetup : ISetup
{
    Task<IReadOnlyDictionary<string, ServiceStatus>> HealthCheckAsync();
    void BuildHealthCheckTable(IReadOnlyDictionary<string, ServiceStatus> serviceStatus);
    bool DetectAndLogFailures(IReadOnlyDictionary<string, ServiceStatus> serviceStatuses);
}
