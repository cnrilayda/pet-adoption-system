namespace PetAdoptionPlatform.Application.DTOs.Auth;

public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

