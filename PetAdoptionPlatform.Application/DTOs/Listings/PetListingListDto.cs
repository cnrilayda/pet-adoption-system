using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Listings;

public class PetListingListDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsShelter { get; set; }
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public bool? IsVaccinated { get; set; }
    public bool? IsNeutered { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public decimal? RequiredAmount { get; set; }
    public decimal? CollectedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

