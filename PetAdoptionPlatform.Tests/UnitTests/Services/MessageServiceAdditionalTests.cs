using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Messages;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class MessageServiceAdditionalTests
{
    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenApplicationNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var sut = new MessageService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.SendMessageAsync(new CreateMessageDto { ApplicationId = Guid.NewGuid(), Content = "hi" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenSenderNotParticipant()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var stranger = SeedHelper.CreateUser("stranger@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);
        await SeedHelper.SaveAsync(ctx, owner, adopter, stranger, listing, app);

        var sut = new MessageService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hi" }, stranger.Id));
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSetReceiverToOwner_WhenAdopterSends()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new MessageService(ctx);

        var dto = await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hello" }, adopter.Id);

        Assert.Equal(adopter.Id, dto.SenderId);
        Assert.Equal(owner.Id, dto.ReceiverId);
        Assert.False(dto.IsRead);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSetReceiverToAdopter_WhenOwnerSends()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new MessageService(ctx);

        var dto = await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hello" }, owner.Id);

        Assert.Equal(owner.Id, dto.SenderId);
        Assert.Equal(adopter.Id, dto.ReceiverId);
    }

    [Fact]
    public async Task MarkMessagesAsReadAsync_ShouldMarkOnlyReceiversMessages()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);

        var m1 = new Message { ApplicationId = app.Id, SenderId = adopter.Id, ReceiverId = owner.Id, Content = "1", IsRead = false };
        var m2 = new Message { ApplicationId = app.Id, SenderId = owner.Id, ReceiverId = adopter.Id, Content = "2", IsRead = false };

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app, m1, m2);

        var sut = new MessageService(ctx);

        // MarkMessagesAsReadAsync expects MESSAGE IDs (not application IDs).
        await sut.MarkMessagesAsReadAsync(new List<Guid> { m1.Id, m2.Id }, owner.Id);

        Assert.True(ctx.Messages.Single(m => m.Id == m1.Id).IsRead);
        Assert.False(ctx.Messages.Single(m => m.Id == m2.Id).IsRead);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnUnreadCount()
    {
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com");
        var u2 = SeedHelper.CreateUser("u2@x.com");
        var listing = SeedHelper.CreateListing(u2.Id, ListingType.Adoption, isApproved: true);
        var app = SeedHelper.CreateApplication(listing.Id, u1.Id, ApplicationStatus.Accepted);

        var m1 = new Message { ApplicationId = app.Id, SenderId = u2.Id, ReceiverId = u1.Id, Content = "1", IsRead = false };
        var m2 = new Message { ApplicationId = app.Id, SenderId = u2.Id, ReceiverId = u1.Id, Content = "2", IsRead = true };

        await SeedHelper.SaveAsync(ctx, u1, u2, listing, app, m1, m2);

        var sut = new MessageService(ctx);

        var count = await sut.GetUnreadCountAsync(u1.Id);

        Assert.Equal(1, count);
    }
}