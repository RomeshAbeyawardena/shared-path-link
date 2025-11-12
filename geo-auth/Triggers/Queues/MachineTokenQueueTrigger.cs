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
        using var memoryStream = new MemoryStream();
        using var textWriter = new StreamWriter(memoryStream);
        textWriter.WriteLine(data);
        memoryStream.Position = 0;
        var machineDataAccess = await JsonSerializer.DeserializeAsync<MachineDataAccessToken>(memoryStream, jsonSerializerOptions, executionContext.CancellationToken);

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

