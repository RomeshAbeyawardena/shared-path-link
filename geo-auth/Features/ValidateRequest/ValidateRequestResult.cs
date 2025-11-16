using GeoAuth.Shared.Models;

namespace geo_auth.Features.ValidateRequest;

public record ValidateRequestResult(ValidateRequest? Result, Exception? Exception = null) : ResultBase<ValidateRequest>(Result, Exception)
{

}
