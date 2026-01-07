namespace PetAdoptionPlatform.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual PetListing Listing { get; set; } = null!;
}

