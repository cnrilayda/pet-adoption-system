namespace PetAdoptionPlatform.Application.DTOs.Donations;

public class DonationDto
{
    public Guid Id { get; set; }
    public Guid DonorId { get; set; }
    public string? DonorName { get; set; }
    public Guid? ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public string PaymentTransactionId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

