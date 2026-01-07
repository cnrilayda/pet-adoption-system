using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Applications;

public class UpdateApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

