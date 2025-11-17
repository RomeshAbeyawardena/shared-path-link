using GeoAuth.Shared.Features.Jwt;
using Microsoft.Extensions.Time.Testing;
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
            .ParseExact("17/11/2025 23:42:08", 
            "dd/MM/yyyy HH:mm:ss",
            CultureInfo.GetCultureInfo("en-gb")));



        _jwtHelper = new(_timeProvider);
    }
    [Test]
    public void WriteToken()
    {

    }
}
