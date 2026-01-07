namespace PetAdoptionPlatform.Application.DTOs.Favorites;

public class FavoriteDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string? ListingSpecies { get; set; }
    public string? ListingBreed { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

