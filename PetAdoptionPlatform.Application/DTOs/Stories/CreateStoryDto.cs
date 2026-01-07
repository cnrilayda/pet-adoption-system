namespace PetAdoptionPlatform.Application.DTOs.Stories;

public class CreateStoryDto
{
    public Guid? ApplicationId { get; set; } // Link to completed adoption (optional)
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}

