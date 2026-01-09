using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Messages;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class MessageServiceTests
{
    [Fact]
    public async Task SendMessageAsync_ShouldCreateMessage_BetweenOwnerAndAdopter()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new MessageService(ctx);

        var msg = await sut.SendMessageAsync(new CreateMessageDto
        {
            ApplicationId = app.Id,
            Content = "hello"
        }, adopter.Id);

        Assert.Equal("hello", msg.Content);
        Assert.Equal(adopter.Id, msg.SenderId);
        Assert.Equal(owner.Id, msg.ReceiverId);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldIncreaseForReceiver()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new MessageService(ctx);
        await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hello" }, adopter.Id);

        var unreadOwner = await sut.GetUnreadCountAsync(owner.Id);
        Assert.Equal(1, unreadOwner);
    }

    [Fact]
    public async Task MarkMessagesAsReadAsync_ShouldClearUnread()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new MessageService(ctx);
        await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hello" }, adopter.Id);

        var messageIds = ctx.Messages
            .Where(m => m.ApplicationId == app.Id && m.ReceiverId == owner.Id && !m.IsRead)
            .Select(m => m.Id)
            .ToList();

        await sut.MarkMessagesAsReadAsync(messageIds, owner.Id);

        var unreadOwner = await sut.GetUnreadCountAsync(owner.Id);
        Assert.Equal(0, unreadOwner);
    }
[Fact]
public async Task SendMessageAsync_ShouldThrow_WhenSenderNotPartOfApplication()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var other = SeedHelper.CreateUser("other@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);

    await SeedHelper.SaveAsync(ctx, owner, adopter, other, listing, app);

    var sut = new MessageService(ctx);

    await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
        sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "hi" }, other.Id));
}

[Fact]
public async Task GetConversationAsync_ShouldReturnMessagesInOrder()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);

    await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

    var sut = new MessageService(ctx);

    await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "1" }, adopter.Id);
    await sut.SendMessageAsync(new CreateMessageDto { ApplicationId = app.Id, Content = "2" }, owner.Id);

    var convo = (await sut.GetConversationAsync(app.Id, adopter.Id)).ToList();

    Assert.Equal(2, convo.Count);
    Assert.Equal("1", convo[0].Content);
    Assert.Equal("2", convo[1].Content);
}
}
