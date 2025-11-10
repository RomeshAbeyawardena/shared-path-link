namespace geo_auth.Models;

public record PasswordSalterRequest
{
    public string? Token { get; set; }
}
