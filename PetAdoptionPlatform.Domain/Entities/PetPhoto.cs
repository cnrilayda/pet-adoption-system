namespace PetAdoptionPlatform.Domain.Entities;

public class PetPhoto : BaseEntity
{
    public Guid ListingId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual PetListing Listing { get; set; } = null!;
}

