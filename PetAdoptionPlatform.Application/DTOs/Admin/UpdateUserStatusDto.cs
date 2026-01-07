namespace PetAdoptionPlatform.Application.DTOs.Admin;

public class UpdateUserStatusDto
{
    public bool? IsActive { get; set; }
    public bool? IsBanned { get; set; }
    public bool? IsShelterVerified { get; set; }
    public string? AdminNotes { get; set; }
}

