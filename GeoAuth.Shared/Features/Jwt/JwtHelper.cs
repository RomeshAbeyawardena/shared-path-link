using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Projectors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace GeoAuth.Shared.Features.Jwt;

public class JwtHelper(ITokenConfiguration tokenConfiguration, TimeProvider timeProvider) : IJwtHelper
{
    private readonly JwtSecurityTokenHandler handler = new();
    
    public async ValueTask<IResult<TToken>> ReadTokenAsync<TToken>(string token, TokenValidationParameters tokenValidationParameters)
    {
        var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);

        if(!result.IsValid)
        {
            return Result.Failed<TToken>(result.Exception);
        }

        var deserialiser = DictionaryProjector<TToken>.Hydrator();
        return Result.Sucessful(deserialiser(result.Claims.ToDictionary()));
    }

    private static SymmetricSecurityKey GetSymmetricSecurityKey(SymmetricSecurityKeyKind kind, ITokenConfiguration tokenConfiguration)
    {
        switch (kind)
        {
            case SymmetricSecurityKeyKind.Encryption:
                var key = Convert.FromBase64String(tokenConfiguration.SigningKey
                   ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError));

                return new SymmetricSecurityKey(key)
                {
                    KeyId = tokenConfiguration.SigningKeyId 
                        ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
                };

                case SymmetricSecurityKeyKind.Signing:
                var enc_key = new Span<byte>(Convert.FromBase64String(tokenConfiguration.EncryptionKey
                    ?? throw new ResponseException("Encryption key is missing", StatusCodes.Status500InternalServerError)))[..32];

                return new SymmetricSecurityKey(enc_key.ToArray())
                {
                    KeyId = tokenConfiguration.EncryptionKeyId
                };

            default:
                throw new IndexOutOfRangeException();
        }
    }

    public IResult<string> WriteToken<TToken>(TToken model, JwtHelperWriterOptions options)
    {
        var utcNow = timeProvider.GetUtcNow();

        var serialiser = DictionaryProjector<TToken>.Serialise();
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = options.IssuerAudienceOptions?.AudienceOverride ?? tokenConfiguration.ValidAudience,
            Issuer = options.IssuerAudienceOptions?.IssuerOverride ?? tokenConfiguration.ValidIssuer,
            Claims = serialiser.Invoke(model),
            NotBefore = utcNow.UtcDateTime,
            Expires = utcNow.UtcDateTime.AddHours(tokenConfiguration.MaximumTokenLifetime
                .GetValueOrDefault(options.DefaultMaximumTokenLifetime.GetValueOrDefault(2))),
            SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Signing, tokenConfiguration), 
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest)
        };

        if (!string.IsNullOrWhiteSpace(tokenConfiguration.EncryptionKey) && options.EncryptToken)
        {
            descriptor.EncryptingCredentials = new EncryptingCredentials(GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Encryption, tokenConfiguration), 
                SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        var token = handler.CreateEncodedJwt(descriptor);
        return Result.Sucessful(token);

    }
}
