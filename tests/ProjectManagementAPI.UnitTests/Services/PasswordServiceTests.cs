using ProjectManagementAPI.Services;

namespace ProjectManagementAPI.UnitTests.Services;

public sealed class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void HashPassword_WhenCalled_ReturnsHashDifferentFromPlainPassword()
    {
        string password = "MySecurePassword123!";

        string hashedPassword = _passwordService.HashPassword(password);

        Assert.False(string.IsNullOrWhiteSpace(hashedPassword));
        Assert.NotEqual(password, hashedPassword);
    }

    [Fact]
    public void HashPassword_WhenCalledTwiceForSamePassword_ReturnsDifferentHashes()
    {
        string password = "MySecurePassword123!";

        string firstHash = _passwordService.HashPassword(password);
        string secondHash = _passwordService.HashPassword(password);

        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordIsCorrect_ReturnsTrue()
    {
        string password = "MySecurePassword123!";
        string hashedPassword = _passwordService.HashPassword(password);

        bool isValid = _passwordService.VerifyPassword(password, hashedPassword);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordIsIncorrect_ReturnsFalse()
    {
        string hashedPassword = _passwordService.HashPassword("MySecurePassword123!");

        bool isValid = _passwordService.VerifyPassword("wrong-password", hashedPassword);

        Assert.False(isValid);
    }
}
