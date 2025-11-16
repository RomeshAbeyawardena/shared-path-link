using geo_auth.Handlers.MachineTokens;
using geo_auth.Models;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace geo_auth.Triggers;

public partial class Queues(IMediator mediator, JsonSerializerOptions jsonSerializerOptions)
{
    [Function("queue-machine-access-token")]
    public async Task RunAsync(
        [QueueTrigger("machine-access-token", Connection = "AzureWebJobsStorage")] string data,
        FunctionContext executionContext)
    {
        var machineDataAccess = JsonSerializer.Deserialize<MachineDataAccessToken>(data, options: jsonSerializerOptions);

        if (machineDataAccess is null)
        {
            return;
        }

        await mediator.Send(new UpdateMachineAccessTokenCommand { 
            PartitionKey = machineDataAccess.PartitionKey,
            Expires = machineDataAccess.Expires,
            Token = machineDataAccess.Token,
            ValidFrom = machineDataAccess.ValidFrom
        }, executionContext.CancellationToken);
    }
}

