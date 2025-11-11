namespace geo_auth.Models;

public record PasswordHasherRequest
{
    public string? Token { get; set; }
}
