using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.Input;

public record PasswordHasherRequest
{
    public string? Token { get; set; }
}

public record PasswordHasherRequestResult(PasswordHasherRequest? Result, Exception? Exception = null) : ResultBase<PasswordHasherRequest>(Result, Exception)
{

}

public record ValidateRequestCommand : IRequest<PasswordHasherRequestResult>
{

}
