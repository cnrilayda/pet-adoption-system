namespace PetAdoptionPlatform.Application.DTOs.Complaints;

public class CreateComplaintDto
{
    public Guid? TargetUserId { get; set; } // User being complained about
    public Guid? TargetListingId { get; set; } // Listing being complained about
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

