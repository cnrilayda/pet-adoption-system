namespace PetAdoptionPlatform.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsShelter { get; set; }
    public bool IsShelterVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsBanned { get; set; }
    public DateTime ExpiresAt { get; set; }
}

