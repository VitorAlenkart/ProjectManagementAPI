using Xunit;
using ProjectManagementAPI.Services;
namespace ProjectManagementAPI.IntegrationsTests;

public class PasswordHasherTests
{
    private PasswordService _passwordService;

    public PasswordHasherTests()
    {
        _passwordService = new PasswordService();
    }

    [InlineData("MySecurePassword123!")]
    [Theory]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword(String password)
    {
        // Act
        string hashedPassword1 = _passwordService.HashPassword(password);
        string hashedPassword2 = _passwordService.HashPassword(password);
        // Assert
        Assert.NotEqual(hashedPassword1, hashedPassword2);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "my_secure_password";
        // Act
        string hashedPassword = _passwordService.HashPassword(password);
        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        string password = "my_secure_password";
        string hashedPassword = _passwordService.HashPassword(password);
        // Act
        bool isValid = _passwordService.VerifyPassword(password, hashedPassword);
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        string password = "my_secure_password";
        string hashedPassword = _passwordService.HashPassword(password);
        // Act
        bool isValid = _passwordService.VerifyPassword("wrong_password", hashedPassword);
        // Assert
        Assert.False(isValid);
    }
    
}
