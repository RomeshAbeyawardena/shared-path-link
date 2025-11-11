namespace geo_auth.Handlers.Passwords;

public record PasswordHasherRequest
{
    public string? Token { get; set; }
}
