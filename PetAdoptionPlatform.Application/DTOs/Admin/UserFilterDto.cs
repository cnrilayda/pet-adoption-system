namespace PetAdoptionPlatform.Application.DTOs.Admin;

public class UserFilterDto
{
    public bool? IsAdmin { get; set; }
    public bool? IsShelter { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsBanned { get; set; }
    public bool? IsShelterVerified { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

