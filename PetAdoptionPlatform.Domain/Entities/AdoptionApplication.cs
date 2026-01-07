using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Domain.Entities;

public class AdoptionApplication : BaseEntity
{
    public Guid ListingId { get; set; }
    public Guid AdopterId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? Message { get; set; }
    public string? AdminNotes { get; set; }
    
    // Navigation properties
    public virtual PetListing Listing { get; set; } = null!;
    public virtual User Adopter { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

