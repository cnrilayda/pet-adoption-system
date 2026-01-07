using PetAdoptionPlatform.Application.DTOs.Auth;

namespace PetAdoptionPlatform.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request, CancellationToken cancellationToken = default);
    Task<string> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}

