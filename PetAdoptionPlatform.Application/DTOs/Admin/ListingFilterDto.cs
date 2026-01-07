using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Application.DTOs.Admin;

public class AdminListingFilterDto
{
    public ListingType? Type { get; set; }
    public bool? IsApproved { get; set; }
    public bool? IsActive { get; set; }
    public Guid? OwnerId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

