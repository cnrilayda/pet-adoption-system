using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Stories;

public class ApproveStoryDto
{
    public StoryStatus Status { get; set; } // Approved or Rejected
    public string? AdminNotes { get; set; }
}

