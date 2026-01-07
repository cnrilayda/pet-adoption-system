namespace PetAdoptionPlatform.Application.DTOs.Ratings;

public class RatingListDto
{
    public Guid Id { get; set; }
    public Guid RaterId { get; set; }
    public string RaterName { get; set; } = string.Empty;
    public Guid RatedUserId { get; set; }
    public string RatedUserName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

