using geo_auth.Models;
using GeoAuth.Shared.Requests.Passwords;
using Konscious.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace geo_auth.Handlers.Passwords;

internal class GeneratePasswordHashCommandHandler(IConfiguration configuration) 
    : IRequestHandler<GeneratePasswordHashCommand, PasswordHashResult>
{
    private static byte[] GenerateSalt(int size = 16)
    {
        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }


    public Task<PasswordHashResult> Handle(GeneratePasswordHashCommand request, CancellationToken cancellationToken)
    {
        var user = request.User;
        if (string.IsNullOrWhiteSpace(user.Salt))
        {
            user.Salt = Convert.ToBase64String(GenerateSalt());
        }

        var hashedPassword = new Argon2id(Encoding.UTF8.GetBytes(user.Secret
            ?? throw new ResponseException("Secret must not be empty", StatusCodes.Status400BadRequest)))
        {
            KnownSecret = Encoding.UTF8.GetBytes(configuration["KnownSecret"]
                ?? throw new ResponseException("KnownSecret is empty", StatusCodes.Status500InternalServerError)),
            Salt = Convert.FromBase64String(user.Salt),
            DegreeOfParallelism = 4,
            MemorySize = 65536,
            Iterations = 4
        }.GetBytes(32);

        return Task.FromResult(new PasswordHashResult(new PasswordHash
        {
            Hash = Convert.ToBase64String(hashedPassword),
            Salt = user.Salt
        }));
    }
}
