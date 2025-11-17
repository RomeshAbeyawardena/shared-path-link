using Azure.Core;
using geo_auth.Configuration;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Projectors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth.Features.BeginAuthentication;

public interface IJwtHelper
{
    IResult<TToken> ReadToken<TToken>(string token, TokenValidationParameters tokenValidationParameters);
    IResult<string> WriteToken<TToken>(TToken model, bool encryptToken = false);
}

public class JwtHelper(IOptions<TokenConfiguration> tokenConfigurationOptions, TimeProvider timeProvider) : IJwtHelper
{
    public IResult<TToken> ReadToken<TToken>(string token, TokenValidationParameters tokenValidationParameters)
    {
        throw new NotImplementedException();
    }

    public IResult<string> WriteToken<TToken>(TToken model, bool encryptToken = false)
    {
        var tokenConfiguration = tokenConfigurationOptions.Value;

        var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
        var signingKeyBytes = Convert.FromBase64String(signingKey);

        var key = new SymmetricSecurityKey(signingKeyBytes)
        {
            KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
        };
        var utcNow = timeProvider.GetUtcNow();

        var dictionaryProjector = DictionaryProjector<TToken>.Create();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = tokenConfiguration.ValidIssuer,
            Audience = tokenConfiguration.ValidAudience,
            Claims = dictionaryProjector.Invoke(model),
            NotBefore = utcNow.UtcDateTime,
            Expires = utcNow.UtcDateTime.AddHours(tokenConfiguration.MaximumTokenLifetime.GetValueOrDefault(2)),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        if (!string.IsNullOrWhiteSpace(tokenConfiguration.EncryptionKey) && encryptToken)
        {
            Span<byte> bytes = new(Convert.FromBase64String(tokenConfiguration.EncryptionKey));
            var keyBytes = bytes.Slice(0, 32);

            key = new SymmetricSecurityKey(keyBytes.ToArray())
            {
                KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
            };

            descriptor.EncryptingCredentials = new EncryptingCredentials(key, SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateEncodedJwt(descriptor);
        return Result.Sucessful(token);

    }
}
