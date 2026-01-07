using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Domain.Entities;

public class Story : BaseEntity
{
    public Guid AuthorId { get; set; }
    public Guid? ApplicationId { get; set; } // Link to completed adoption
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public StoryStatus Status { get; set; } = StoryStatus.Pending;
    public string? AdminNotes { get; set; }
    
    // Navigation properties
    public virtual User Author { get; set; } = null!;
    public virtual AdoptionApplication? Application { get; set; }
}

