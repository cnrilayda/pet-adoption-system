namespace PetAdoptionPlatform.Application.DTOs.Donations;

public class DonationSummaryDto
{
    public decimal TotalDonations { get; set; }
    public int DonationCount { get; set; }
    public decimal? ListingTotalDonations { get; set; }
    public int? ListingDonationCount { get; set; }
}

