using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Stories;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class StoryServiceTests
{
    [Fact]
    public async Task CreateStoryAsync_ShouldCreateNotApproved_ByDefault()
    {
        using var ctx = TestDbContextFactory.Create();
        var author = SeedHelper.CreateUser("a@x.com");
        await SeedHelper.SaveAsync(ctx, author);

        var sut = new StoryService(ctx);

        var story = await sut.CreateStoryAsync(new CreateStoryDto
        {
            Title = "My story",
            Content = "Content"
        }, author.Id);

        Assert.Equal(StoryStatus.Pending, story.Status);
        Assert.Equal(author.Id, story.AuthorId);
    }

    [Fact]
    public async Task ApproveStoryAsync_ShouldSetApproved()
    {
        using var ctx = TestDbContextFactory.Create();
        var author = SeedHelper.CreateUser("a@x.com");
        await SeedHelper.SaveAsync(ctx, author);

        var sut = new StoryService(ctx);
        var created = await sut.CreateStoryAsync(new CreateStoryDto { Title = "t", Content = "c" }, author.Id);

        var approved = await sut.ApproveStoryAsync(created.Id, new ApproveStoryDto { Status = StoryStatus.Approved, AdminNotes = "ok" });

        Assert.Equal(StoryStatus.Approved, approved.Status);

        var list = (await sut.GetApprovedStoriesAsync()).ToList();
        Assert.Single(list);
        Assert.Equal(created.Id, list[0].Id);
    }
}
