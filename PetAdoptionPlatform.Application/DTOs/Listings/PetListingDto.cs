using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Listings;

public class PetListingDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsShelter { get; set; }
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Pet details
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public bool? IsVaccinated { get; set; }
    public bool? IsNeutered { get; set; }
    
    // Location
    public string? City { get; set; }
    public string? District { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public bool IsPaused { get; set; }
    
    // Help request specific
    public decimal? RequiredAmount { get; set; }
    public decimal? CollectedAmount { get; set; }
    
    // Photos
    public List<string> PhotoUrls { get; set; } = new();
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Counts
    public int ApplicationCount { get; set; }
    public int FavoriteCount { get; set; }
}

