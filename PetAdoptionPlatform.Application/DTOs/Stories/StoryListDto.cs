using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Stories;

public class StoryListDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public StoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

