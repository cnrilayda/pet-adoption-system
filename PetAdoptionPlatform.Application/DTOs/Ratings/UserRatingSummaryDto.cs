namespace PetAdoptionPlatform.Application.DTOs.Ratings;

public class UserRatingSummaryDto
{
    public Guid UserId { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int Rating1 { get; set; }
    public int Rating2 { get; set; }
    public int Rating3 { get; set; }
    public int Rating4 { get; set; }
    public int Rating5 { get; set; }
}

