using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class EligibilityFormServiceAdditionalTests
{
    [Fact]
    public async Task HasFormAsync_ShouldReturnFalse_WhenNoForm()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new EligibilityFormService(ctx);

        Assert.False(await sut.HasFormAsync(user.Id));
    }

    [Fact]
    public async Task HasFormAsync_ShouldReturnTrue_WhenFormExists()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new EligibilityFormService(ctx);

        await sut.CreateFormAsync(
            new CreateEligibilityFormDto { HouseholdMembers = 2 },
            user.Id
        );

        Assert.True(await sut.HasFormAsync(user.Id));
    }

    [Fact]
    public async Task GetFormByIdAsync_ShouldThrow_WhenNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new EligibilityFormService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.GetFormByIdAsync(Guid.NewGuid(), user.Id)
        );
    }

    [Fact]
    public async Task CreateFormAsync_ShouldAllowCreation_WhenUserInactive()
    {
        // EligibilityFormService does not enforce "IsActive" on create/update; it only checks that the user exists.
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com", isActive: false);
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new EligibilityFormService(ctx);

        var dto = await sut.CreateFormAsync(
            new CreateEligibilityFormDto { HouseholdMembers = 2 },
            user.Id
        );

        Assert.Equal(user.Id, dto.UserId);
    }

    [Fact]
    public async Task UpdateFormAsync_ShouldThrow_WhenUserHasNoForm()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new EligibilityFormService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.UpdateFormAsync(
                new UpdateEligibilityFormDto { HouseholdMembers = 3 },
                user.Id
            )
        );
    }
}