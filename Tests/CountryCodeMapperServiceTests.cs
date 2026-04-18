using BankProfiles.Web.Services;

namespace BankProfiles.Tests;

public class CountryCodeMapperServiceTests
{
    private readonly CountryCodeMapperService _sut = new();

    [Theory]
    [InlineData("United Kingdom", "GB")]
    [InlineData("United States", "US")]
    [InlineData("Germany", "DE")]
    [InlineData("Japan", "JP")]
    [InlineData("UAE", "AE")]
    public void TryGetIso2Code_MapsKnownCountryNames(string country, string expectedCode)
    {
        var result = _sut.TryGetIso2Code(country, out var actualCode);

        Assert.True(result);
        Assert.Equal(expectedCode, actualCode);
    }

    [Theory]
    [InlineData("us", "US")]
    [InlineData("de", "DE")]
    [InlineData("uk", "GB")]
    public void TryGetIso2Code_MapsIsoLikeCodes(string input, string expectedCode)
    {
        var result = _sut.TryGetIso2Code(input, out var actualCode);

        Assert.True(result);
        Assert.Equal(expectedCode, actualCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Unknownland")]
    public void TryGetIso2Code_ReturnsFalse_WhenCountryCannotBeMapped(string input)
    {
        var result = _sut.TryGetIso2Code(input, out var actualCode);

        Assert.False(result);
        Assert.Equal(string.Empty, actualCode);
    }
}
