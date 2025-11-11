using GeoAuth.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GeoAuth.Shared.Requests.Input;

public record ValidateRequest
{
    public string? Token { get; set; }
}

public record ValidateRequestResult(ValidateRequest? Result, Exception? Exception = null) : ResultBase<ValidateRequest>(Result, Exception)
{

}

public class ValidateRequestCommand : IRequest<ValidateRequestResult>
{
    public IEnumerable<string> AcceptableEncodings { get; init; } = [];
    public HttpContext? HttpContext { get; init; }
}
