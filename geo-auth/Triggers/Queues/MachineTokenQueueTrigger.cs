using geo_auth.Models;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace geo_auth.Triggers;

public partial class Queues(IMediator mediator)
{
    public async Task RunAsync(
        [QueueTrigger("machine-data-access", Connection = "AzureWebJobsStorage")] MachineDataAccessToken machineDataAccess,
        FunctionContext executionContext)
    {
        await mediator.Publish(new UpdateMachineQueryAccessTokenNotification { 
            PartitionKey = machineDataAccess.PartitionKey,
            Expires = machineDataAccess.Expires,
            Token = machineDataAccess.Token,
            ValidFrom = machineDataAccess.ValidFrom
        }, executionContext.CancellationToken);
    }
}

