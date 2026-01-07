namespace PetAdoptionPlatform.Application.DTOs.Donations;

public class DonationListDto
{
    public Guid Id { get; set; }
    public Guid DonorId { get; set; }
    public string? DonorName { get; set; }
    public Guid? ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public decimal Amount { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
}

