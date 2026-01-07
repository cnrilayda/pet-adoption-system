using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    Guid? GetUserIdFromToken(string token);
}

