using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Listings;

public class CreatePetListingDto
{
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Pet details
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? Age { get; set; } // in months
    public string? Gender { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public bool? IsVaccinated { get; set; }
    public bool? IsNeutered { get; set; }
    
    // Location
    public string? City { get; set; }
    public string? District { get; set; }
    
    // Help request specific
    public decimal? RequiredAmount { get; set; }
    
    // Photos (will be handled separately for now, URLs as strings)
    public List<string>? PhotoUrls { get; set; }
}

