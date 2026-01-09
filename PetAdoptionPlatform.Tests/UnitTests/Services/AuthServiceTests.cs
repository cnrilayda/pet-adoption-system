using System.Threading;
using System.Threading.Tasks;
using Moq;
using PetAdoptionPlatform.Application.DTOs.Auth;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_AndReturnToken()
    {
        using var ctx = TestDbContextFactory.Create();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenService>();

        hasher.Setup(h => h.HashPassword("pw")).Returns("HASHED");
        jwt.Setup(j => j.GenerateToken(It.IsAny<PetAdoptionPlatform.Domain.Entities.User>())).Returns("TOKEN");

        var sut = new AuthService(ctx, hasher.Object, jwt.Object);

        var req = new RegisterRequestDto
        {
            Email = "a@b.com",
            Password = "pw",
            FirstName = "A",
            LastName = "B",
            PhoneNumber = "1",
            City = "Istanbul"
        };

        var res = await sut.RegisterAsync(req, CancellationToken.None);

        Assert.Equal("TOKEN", res.Token);
        Assert.Equal("a@b.com", res.Email);
        Assert.NotEqual(System.Guid.Empty, res.UserId);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailExists()
    {
        using var ctx = TestDbContextFactory.Create();
        await SeedHelper.SaveAsync(ctx, SeedHelper.CreateUser("dup@x.com"));

        var sut = new AuthService(ctx, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenService>());

        var req = new RegisterRequestDto
        {
            Email = "dup@x.com",
            Password = "pw",
            FirstName = "A",
            LastName = "B",
            PhoneNumber = "1",
            City = "Istanbul"
        };

        await Assert.ThrowsAsync<System.InvalidOperationException>(() => sut.RegisterAsync(req));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        user.PasswordHash = "HASHED";
        await SeedHelper.SaveAsync(ctx, user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.VerifyPassword("pw", "HASHED")).Returns(true);

        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<PetAdoptionPlatform.Domain.Entities.User>())).Returns("TOKEN");

        var sut = new AuthService(ctx, hasher.Object, jwt.Object);

        var res = await sut.LoginAsync(new LoginRequestDto { Email = "u@x.com", Password = "pw" });

        Assert.Equal("TOKEN", res.Token);
        Assert.Equal(user.Id, res.UserId);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserInactive()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com", isActive: false);
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new AuthService(ctx, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenService>());

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "u@x.com", Password = "pw" }));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var sut = new AuthService(ctx, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenService>());

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "missing@x.com", Password = "pw" }));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordInvalid()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        user.PasswordHash = "HASHED";
        await SeedHelper.SaveAsync(ctx, user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.VerifyPassword("pw", "HASHED")).Returns(false);

        var sut = new AuthService(ctx, hasher.Object, Mock.Of<IJwtTokenService>());

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequestDto { Email = "u@x.com", Password = "pw" }));
    }
}
