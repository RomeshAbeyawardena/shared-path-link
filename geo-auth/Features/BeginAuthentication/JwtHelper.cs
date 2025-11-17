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
    IResult<string> WriteToken<TToken>(TToken model, JwtHelperWriterOptions options);
}

public record IssuerAudienceOptions(string? AudienceOverride = null, string? IssuerOverride = null);

public record JwtHelperWriterOptions(bool EncryptToken = false, 
    IssuerAudienceOptions? IssuerAudienceOptions = null, 
    int? DefaultMaximumTokenLifetime = null);

public class JwtHelper(IOptions<TokenConfiguration> tokenConfigurationOptions, TimeProvider timeProvider) : IJwtHelper
{
    public IResult<TToken> ReadToken<TToken>(string token, TokenValidationParameters tokenValidationParameters)
    {
        throw new NotImplementedException();
    }

    public IResult<string> WriteToken<TToken>(TToken model, JwtHelperWriterOptions options)
    {
        var tokenConfiguration = tokenConfigurationOptions.Value;

        var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
        var signingKeyBytes = Convert.FromBase64String(signingKey);

        var key = new SymmetricSecurityKey(signingKeyBytes)
        {
            KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
        };
        var utcNow = timeProvider.GetUtcNow();

        var dictionaryProjector = DictionaryProjector<TToken>.Serialise();
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = options.IssuerAudienceOptions?.AudienceOverride ?? tokenConfiguration.ValidAudience,
            Issuer = options.IssuerAudienceOptions?.IssuerOverride ?? tokenConfiguration.ValidIssuer,
            Claims = dictionaryProjector.Invoke(model),
            NotBefore = utcNow.UtcDateTime,
            Expires = utcNow.UtcDateTime.AddHours(tokenConfiguration.MaximumTokenLifetime
                .GetValueOrDefault(options.DefaultMaximumTokenLifetime.GetValueOrDefault(2))),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        if (!string.IsNullOrWhiteSpace(tokenConfiguration.EncryptionKey) && options.EncryptToken)
        {
            Span<byte> bytes = new(Convert.FromBase64String(tokenConfiguration.EncryptionKey));
            var keyBytes = bytes.Slice(0, 32);

            key = new SymmetricSecurityKey(keyBytes.ToArray())
            {
                KeyId = tokenConfiguration.EncryptionKeyId 
                ?? throw new ResponseException("Encrypting key ID missing", StatusCodes.Status500InternalServerError)
            };

            descriptor.EncryptingCredentials = new EncryptingCredentials(key, SecurityAlgorithms.Aes256KW,
                SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateEncodedJwt(descriptor);
        return Result.Sucessful(token);

    }
}
