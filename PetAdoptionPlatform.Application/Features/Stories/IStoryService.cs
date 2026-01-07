using PetAdoptionPlatform.Application.DTOs.Stories;

namespace PetAdoptionPlatform.Application.Features.Stories;

public interface IStoryService
{
    Task<StoryDto> CreateStoryAsync(CreateStoryDto request, Guid authorId, CancellationToken cancellationToken = default);
    Task<StoryDto> GetStoryByIdAsync(Guid storyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StoryListDto>> GetApprovedStoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<StoryListDto>> GetMyStoriesAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<StoryDto> ApproveStoryAsync(Guid storyId, ApproveStoryDto request, CancellationToken cancellationToken = default);
}

