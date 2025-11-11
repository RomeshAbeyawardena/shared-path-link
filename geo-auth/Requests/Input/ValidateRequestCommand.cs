using GeoAuth.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GeoAuth.Shared.Requests.Input;

public record PasswordHasherRequest
{
    public string? Token { get; set; }
}

public record PasswordHasherRequestResult(PasswordHasherRequest? Result, Exception? Exception = null) : ResultBase<PasswordHasherRequest>(Result, Exception)
{

}

public class ValidateRequestCommand : IRequest<PasswordHasherRequestResult>
{
    public HttpContext? HttpContext { get; init; }
}
