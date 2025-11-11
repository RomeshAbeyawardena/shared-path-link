namespace GeoAuth.Shared.Models;

public record UserResult(User? Result, Exception? Exception = null) : ResultBase<User>(Result, Exception)
{

}