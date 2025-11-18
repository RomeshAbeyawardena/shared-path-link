using GeoAuth.Shared.Features.Jwt;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Logging;
using Moq;
using System.Globalization;


namespace GeoAuth.Tests;

[TestFixture]
internal class JwtHelperTests
{
    private TimeProvider _timeProvider;
    private JwtHelper _jwtHelper;
    [SetUp]

    public void SetUp()
    {
        _timeProvider = new FakeTimeProvider(DateTimeOffset
            .ParseExact("18/11/2025 13:40:08", 
            "dd/MM/yyyy HH:mm:ss",
            CultureInfo.GetCultureInfo("en-gb")));

        var config = new Mock<ITokenConfiguration>();

        config.SetupGet(x => x.EncryptionKey)
            .Returns("MGNjMDg0YzgtNTdlYi00ZmU0LWExOWItNjZhYzE4NmIyNWJi");

        config.SetupGet(x => x.EncryptionKeyId)
            .Returns("ODM5ZWExMjgtMWYxZS00Y2M4LTlkMzMtMjZiZTc2NzIyM2M4");

        config.SetupGet(x => x.MaximumTokenLifetime)
            .Returns(2);

        config.SetupGet(x => x.SigningKey)
            .Returns("ODM5ZWExMjgtMWYxZS00Y2M4LTlkMzMtMjZiZTc2NzIyM2M4");

        config.SetupGet(x => x.SigningKeyId)
            .Returns("N2EzZDJlOTctN2NlMS00NmJlLWIwNWYtODFkY2MxZGFhOTlh");

        config.SetupGet(x => x.ValidAudience)
            .Returns("http://localhost:4050");

        config.SetupGet(x => x.ValidIssuer)
            .Returns("http://localhost:4000");

        _jwtHelper = new(config.Object, _timeProvider);
    }

    [Test]
    public async Task WriteToken()
    {
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;

        var token = _jwtHelper.WriteToken(new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Susan Hall",
            Address = "22 Crossbridge Avenue",
            City = "Edinburgh",
            Region = "Scotland",
            PostalCode = "ES1 NHX"
        }, new JwtHelperWriterOptions(true));

        Assert.That(token.IsSuccess, Is.True);

        var result = await _jwtHelper.ReadTokenAsync<CustomerDto>(token.Result!, _jwtHelper.DefaultParameters(true, true));
        Assert.That(result.IsSuccess, Is.True, () => result.Exception?.Message ?? string.Empty);
    }
}
