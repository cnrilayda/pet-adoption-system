namespace PetAdoptionPlatform.Application.DTOs.Ratings;

public class CreateRatingDto
{
    public Guid ApplicationId { get; set; } // Must be a completed adoption
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
}

