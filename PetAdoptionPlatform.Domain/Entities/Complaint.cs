using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Domain.Entities;

public class Complaint : BaseEntity
{
    public Guid ComplainantId { get; set; }
    public Guid? TargetUserId { get; set; } // User being complained about
    public Guid? TargetListingId { get; set; } // Listing being complained about
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public string? AdminNotes { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Navigation properties
    public virtual User Complainant { get; set; } = null!;
    public virtual User? TargetUser { get; set; }
    public virtual PetListing? TargetListing { get; set; }
}

