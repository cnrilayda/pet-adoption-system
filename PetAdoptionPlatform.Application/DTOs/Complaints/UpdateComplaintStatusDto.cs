using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Complaints;

public class UpdateComplaintStatusDto
{
    public ComplaintStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? ResolutionNotes { get; set; }
}

