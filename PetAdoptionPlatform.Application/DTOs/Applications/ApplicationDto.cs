using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public Guid AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public string AdopterEmail { get; set; } = string.Empty;
    public string? AdopterPhone { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? Message { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Eligibility form info (if available)
    public bool HasEligibilityForm { get; set; }
    public Guid? EligibilityFormId { get; set; }
}

