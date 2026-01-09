using System.Collections.Generic;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class EligibilityFormServiceTests
{
    [Fact]
    public async Task CreateFormAsync_ShouldCreateForm_ForActiveUser()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, u);

        var sut = new EligibilityFormService(ctx);
        var created = await sut.CreateFormAsync(new CreateEligibilityFormDto { LivingType = "Apartment" }, u.Id);

        Assert.Equal(u.Id, created.UserId);
        Assert.True(await sut.HasFormAsync(u.Id));
    }

    [Fact]
    public async Task CreateFormAsync_ShouldThrow_WhenFormAlreadyExists()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        var form = SeedHelper.CreateEligibilityForm(u.Id);
        await SeedHelper.SaveAsync(ctx, u, form);

        var sut = new EligibilityFormService(ctx);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() => sut.CreateFormAsync(new CreateEligibilityFormDto(), u.Id));
    }

    [Fact]
    public async Task UpdateFormAsync_ShouldUpdate_WhenOwner()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        var form = SeedHelper.CreateEligibilityForm(u.Id);
        await SeedHelper.SaveAsync(ctx, u, form);

        var sut = new EligibilityFormService(ctx);

        var updated = await sut.UpdateFormAsync(new UpdateEligibilityFormDto { LivingType = "House" }, u.Id);

        Assert.Equal("House", updated.LivingType);
    }

    [Fact]
    public async Task UpdateFormAsync_ShouldThrow_WhenNoExistingForm()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, u);

        var sut = new EligibilityFormService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateFormAsync(new UpdateEligibilityFormDto(), u.Id));
    }

    [Fact]
    public async Task GetFormByUserIdAsync_ShouldReturnForm()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        var form = SeedHelper.CreateEligibilityForm(u.Id);
        await SeedHelper.SaveAsync(ctx, u, form);

        var sut = new EligibilityFormService(ctx);

        var got = await sut.GetFormByUserIdAsync(u.Id);

        Assert.Equal(u.Id, got.UserId);
    }
}
