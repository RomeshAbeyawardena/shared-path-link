namespace geo_auth.Handlers.Passwords;

internal record PasswordConfiguration(string KnownSecret, 
    int? DegreeOfParallelism,
    int? MemorySize,
    int? Iterations,
    int? KeySize,
    int? SaltSize);
