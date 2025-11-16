using MediatR;
using Microsoft.AspNetCore.Http;

namespace geo_auth.Features.ValidateRequest;

public class ValidateRequestCommand : IRequest<ValidateRequestResult>
{
    public IEnumerable<string> AcceptableEncodings { get; init; } = [];
    public HttpContext? HttpContext { get; init; }
}
