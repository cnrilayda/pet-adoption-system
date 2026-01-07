namespace PetAdoptionPlatform.Application.DTOs.Admin;

public class UserManagementDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsShelter { get; set; }
    public bool IsShelterVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ListingCount { get; set; }
    public int ApplicationCount { get; set; }
}

