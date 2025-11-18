using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Projectors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace GeoAuth.Shared.Features.Jwt;

public class JwtHelper(ITokenConfiguration tokenConfiguration, TimeProvider timeProvider) : IJwtHelper
{

    private static SymmetricSecurityKey GetSymmetricSecurityKey(SymmetricSecurityKeyKind kind, ITokenConfiguration tokenConfiguration)
    {
        switch (kind)
        {
            case SymmetricSecurityKeyKind.Encryption:

                var rawKey = Convert.FromBase64String(tokenConfiguration.EncryptionKey
                    ?? throw new ResponseException("Encryption key is missing", StatusCodes.Status500InternalServerError));

                if (rawKey.Length < 32)
                {
                    throw new IndexOutOfRangeException("Encryption key must have a minimum length of 32 bytes");
                }

                var enc_key = new Span<byte>(rawKey)[..32];

                return new SymmetricSecurityKey(enc_key.ToArray())
                {
                    KeyId = tokenConfiguration.EncryptionKeyId
                        ?? throw new ResponseException("Encryption key ID missing", StatusCodes.Status500InternalServerError)
                };
            case SymmetricSecurityKeyKind.Signing:
                var key = Convert.FromBase64String(tokenConfiguration.SigningKey
                   ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError));
                
                return new SymmetricSecurityKey(key)
                {
                    KeyId = tokenConfiguration.SigningKeyId
                        ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
                };

            default:
                throw new IndexOutOfRangeException();
        }
    }

    private readonly JwtSecurityTokenHandler handler = new();

    private EncryptingCredentials GetEncryptingCredentials()
    {
        return new EncryptingCredentials(GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Encryption, tokenConfiguration),
                SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512);
    }

    private SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Signing, tokenConfiguration),
                SecurityAlgorithms.HmacSha256,
                SecurityAlgorithms.Sha256Digest);
    }

    public async ValueTask<IResult<TToken>> ReadTokenAsync<TToken>(string token, TokenValidationParameters tokenValidationParameters)
    {
        var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);

        if (!result.IsValid)
        {
            return Result.Failed<TToken>(result.Exception);
        }

        var deserialiser = DictionaryProjector<TToken>.Hydrator();
        return Result.Sucessful(deserialiser(result.Claims.ToDictionary()));
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
            SigningCredentials = GetSigningCredentials()
        };

        if (!string.IsNullOrWhiteSpace(tokenConfiguration.EncryptionKey) && options.EncryptToken)
        {
            descriptor.EncryptingCredentials = GetEncryptingCredentials();
        }

        var token = handler.CreateEncodedJwt(descriptor);
        return Result.Sucessful(token);

    }

    public TokenValidationParameters DefaultParameters(bool requiresSignedCredentials, bool requiresEncryptionCredentials)
    {

        var parameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = tokenConfiguration.ValidAudience,
            ValidateIssuer = true,
            ValidIssuer = tokenConfiguration.ValidIssuer,
            ValidateIssuerSigningKey = true,
            //ValidateTokenReplay = true
        };

        var validAlgorithms = new List<string>();
        if (requiresSignedCredentials)
        {
            parameters.IssuerSigningKey = GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Signing, tokenConfiguration);
            validAlgorithms.AddRange([SecurityAlgorithms.HmacSha256, SecurityAlgorithms.Sha256Digest]);
        }

        if (requiresEncryptionCredentials)
        {
            parameters.TokenDecryptionKey = GetSymmetricSecurityKey(SymmetricSecurityKeyKind.Encryption, tokenConfiguration);
            validAlgorithms.AddRange([SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512]);
        }

        parameters.ValidAlgorithms = validAlgorithms;

        return parameters;
    }
}
