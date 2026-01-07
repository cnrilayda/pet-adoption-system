using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Stories;
using PetAdoptionPlatform.Application.Features.Stories;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class StoryService : IStoryService
{
    private readonly ApplicationDbContext _context;

    public StoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StoryDto> CreateStoryAsync(CreateStoryDto request, Guid authorId, CancellationToken cancellationToken = default)
    {
        // Verify author exists
        var author = await _context.Users.FindAsync(new object[] { authorId }, cancellationToken);
        if (author == null || !author.IsActive)
        {
            throw new UnauthorizedAccessException("Author not found or inactive.");
        }

        // If linked to application, verify it exists and is completed
        if (request.ApplicationId.HasValue)
        {
            var application = await _context.AdoptionApplications
                .Include(a => a.Listing)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId.Value, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException("Application not found.");
            }

            if (application.Status != ApplicationStatus.Completed)
            {
                throw new InvalidOperationException("Story can only be linked to completed adoptions.");
            }

            if (application.AdopterId != authorId && application.Listing.OwnerId != authorId)
            {
                throw new UnauthorizedAccessException("You can only create stories for your own completed adoptions.");
            }
        }

        var story = new Story
        {
            AuthorId = authorId,
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            Content = request.Content,
            PhotoUrl = request.PhotoUrl,
            Status = StoryStatus.Pending // Admin approval required
        };

        _context.Stories.Add(story);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetStoryByIdAsync(story.Id, cancellationToken);
    }

    public async Task<StoryDto> GetStoryByIdAsync(Guid storyId, CancellationToken cancellationToken = default)
    {
        var story = await _context.Stories
            .Include(s => s.Author)
            .FirstOrDefaultAsync(s => s.Id == storyId, cancellationToken);

        if (story == null)
        {
            throw new KeyNotFoundException("Story not found.");
        }

        return MapToDto(story);
    }

    public async Task<IEnumerable<StoryListDto>> GetApprovedStoriesAsync(CancellationToken cancellationToken = default)
    {
        var stories = await _context.Stories
            .Include(s => s.Author)
            .Where(s => s.Status == StoryStatus.Approved && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return stories.Select(s => new StoryListDto
        {
            Id = s.Id,
            AuthorId = s.AuthorId,
            AuthorName = $"{s.Author.FirstName} {s.Author.LastName}",
            Title = s.Title,
            PhotoUrl = s.PhotoUrl,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        });
    }

    public async Task<IEnumerable<StoryListDto>> GetMyStoriesAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        var stories = await _context.Stories
            .Include(s => s.Author)
            .Where(s => s.AuthorId == authorId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return stories.Select(s => new StoryListDto
        {
            Id = s.Id,
            AuthorId = s.AuthorId,
            AuthorName = $"{s.Author.FirstName} {s.Author.LastName}",
            Title = s.Title,
            PhotoUrl = s.PhotoUrl,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        });
    }

    public async Task<StoryDto> ApproveStoryAsync(Guid storyId, ApproveStoryDto request, CancellationToken cancellationToken = default)
    {
        var story = await _context.Stories
            .FirstOrDefaultAsync(s => s.Id == storyId, cancellationToken);

        if (story == null)
        {
            throw new KeyNotFoundException("Story not found.");
        }

        if (request.Status != StoryStatus.Approved && request.Status != StoryStatus.Rejected)
        {
            throw new InvalidOperationException("Status must be either Approved or Rejected.");
        }

        story.Status = request.Status;
        story.AdminNotes = request.AdminNotes;
        story.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetStoryByIdAsync(storyId, cancellationToken);
    }

    private StoryDto MapToDto(Story story)
    {
        return new StoryDto
        {
            Id = story.Id,
            AuthorId = story.AuthorId,
            AuthorName = $"{story.Author.FirstName} {story.Author.LastName}",
            ApplicationId = story.ApplicationId,
            Title = story.Title,
            Content = story.Content,
            PhotoUrl = story.PhotoUrl,
            Status = story.Status,
            AdminNotes = story.AdminNotes,
            CreatedAt = story.CreatedAt,
            UpdatedAt = story.UpdatedAt
        };
    }
}

