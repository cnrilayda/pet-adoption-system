namespace PetAdoptionPlatform.Application.DTOs.Donations;

public class CreateDonationDto
{
    public Guid? ListingId { get; set; } // Null for general shelter donations
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

