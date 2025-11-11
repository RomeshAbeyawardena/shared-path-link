# secrets.json Setup
```
{
    "token": {
        "SigningKey": "<base64.key>",
        "SigningKeyId": "<guid>",
        "ValidAudience": "http://localhost:7018",
        "ValidIssuer": "http://localhost:8787"
    },
    "password": {
        "KnownSecret": "<base64.key>",
        "DegreeOfParallelism": 4,
        "MemorySize": 65536,
        "Iterations": 4,
        "KeySize": 32,
        "SaltSize": 16
    }
}
```