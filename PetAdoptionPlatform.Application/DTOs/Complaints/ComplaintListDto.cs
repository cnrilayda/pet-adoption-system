using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Complaints;

public class ComplaintListDto
{
    public Guid Id { get; set; }
    public Guid ComplainantId { get; set; }
    public string ComplainantName { get; set; } = string.Empty;
    public Guid? TargetUserId { get; set; }
    public Guid? TargetListingId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

