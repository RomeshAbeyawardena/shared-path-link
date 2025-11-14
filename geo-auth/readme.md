# secrets.json Setup
```
{
    "machine": {
        "MachineTokenTableName": "machinetokens",
        "MachineAccessTokenTableName": "machineaccesstoken",
        "MachineAccessTokenQueueName": "machine-access-token"
    },
    "password": {
        "KnownSecret": "<key.base64>",
        "DegreeOfParallelism": 4,
        "MemorySize": 65536,
        "Iterations": 4,
        "KeySize": 32,
        "SaltSize": 16
    },
    "token": {
        "SigningKey": "<key.base64>",
        "SigningKeyId": "{guid}",
        "ValidAudience": "http://localhost:7018",
        "ValidIssuer": "http://localhost:8787",
        "MaximumTokenLifetime": 2
    },
    "values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true"
    }
}
```