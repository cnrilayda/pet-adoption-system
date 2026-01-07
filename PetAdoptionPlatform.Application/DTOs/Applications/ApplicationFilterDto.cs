using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Applications;

public class ApplicationFilterDto
{
    public Guid? ListingId { get; set; }
    public Guid? AdopterId { get; set; }
    public ApplicationStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

