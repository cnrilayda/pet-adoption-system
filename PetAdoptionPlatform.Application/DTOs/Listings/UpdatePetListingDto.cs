namespace PetAdoptionPlatform.Application.DTOs.Listings;

public class UpdatePetListingDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    
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
    public bool? IsPaused { get; set; }
    
    // Help request specific
    public decimal? RequiredAmount { get; set; }
}

