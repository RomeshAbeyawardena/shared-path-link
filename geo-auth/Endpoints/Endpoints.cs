using Azure.Core;
using Microsoft.AspNetCore.Http;

namespace geo_auth;

public static partial class Endpoints
{
    public static Guid? GetAutomationId(IHeaderDictionary headers)
    {
        return headers.TryGetValue("automation-id", out var automationIdValue)
            && Guid.TryParse(automationIdValue, out var id) ? id : null;
    }
}
