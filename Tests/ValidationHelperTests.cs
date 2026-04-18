using BankProfiles.Web.Services;

namespace BankProfiles.Tests;

public class ValidationHelperTests
{
    [Theory]
    [InlineData("/images/banks/alpha-logo.png")]
    [InlineData("/images/banks/beta-icon.svg")]
    [InlineData("/images/banks/teams/bank-gamma-logo.webp")]
    public void IsValidBankAssetPath_ReturnsTrue_ForValidPaths(string path)
    {
        Assert.True(ValidationHelper.IsValidBankAssetPath(path));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("https://cdn.example.com/logo.png")]
    [InlineData("/images/logo.png")]
    [InlineData("/images/banks/../logo.png")]
    [InlineData("/images/banks/logo.exe")]
    [InlineData("/images/banks/logo")]
    [InlineData("/images/banks/logo.WEBP")]
    [InlineData("/images/banks/logo.png?version=1")]
    [InlineData("/images\\banks\\logo.png")]
    public void IsValidBankAssetPath_ReturnsFalse_ForInvalidPaths(string? path)
    {
        Assert.False(ValidationHelper.IsValidBankAssetPath(path));
    }

    [Fact]
    public void NormalizeBankAssetPath_ReturnsTrimmedPath_WhenPathIsValid()
    {
        var normalized = ValidationHelper.NormalizeBankAssetPath("  /images/banks/alpha-logo.png  ");
        Assert.Equal("/images/banks/alpha-logo.png", normalized);
    }

    [Fact]
    public void NormalizeBankAssetPath_ReturnsNull_WhenPathIsInvalid()
    {
        var normalized = ValidationHelper.NormalizeBankAssetPath("/images/banks/../alpha-logo.png");
        Assert.Null(normalized);
    }
}
