using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Domain.Entities;

public class PetListing : BaseEntity
{
    public Guid OwnerId { get; set; }
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Pet details
    public string? Species { get; set; } // Cat, Dog, Other
    public string? Breed { get; set; }
    public int? Age { get; set; } // in months
    public string? Gender { get; set; } // Male, Female, Unknown
    public string? Size { get; set; } // Small, Medium, Large
    public string? Color { get; set; }
    public bool? IsVaccinated { get; set; }
    public bool? IsNeutered { get; set; }
    
    // Location
    public string? City { get; set; }
    public string? District { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsApproved { get; set; } = false; // Admin approval for listings
    public bool IsPaused { get; set; } = false;
    
    // Help request specific
    public decimal? RequiredAmount { get; set; } // For help/vet support requests
    public decimal? CollectedAmount { get; set; } = 0;
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<AdoptionApplication> Applications { get; set; } = new List<AdoptionApplication>();
    public virtual ICollection<PetPhoto> Photos { get; set; } = new List<PetPhoto>();
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}

