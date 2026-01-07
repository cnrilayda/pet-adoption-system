namespace PetAdoptionPlatform.Domain.Entities;

public class Donation : BaseEntity
{
    public Guid DonorId { get; set; }
    public Guid? ListingId { get; set; } // Null for general shelter donations
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; } = false;
    public string PaymentTransactionId { get; set; } = string.Empty; // From PaymentGateway
    public string PaymentStatus { get; set; } = string.Empty; // Pending, Completed, Failed
    
    // Navigation properties
    public virtual User Donor { get; set; } = null!;
    public virtual PetListing? Listing { get; set; }
}

