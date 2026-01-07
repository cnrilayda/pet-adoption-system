using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Auth;
using PetAdoptionPlatform.Application.Features.Auth;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            City = request.City,
            IsShelter = request.IsShelter,
            IsActive = true,
            IsAdmin = false,
            IsBanned = false,
            IsShelterVerified = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsAdmin = user.IsAdmin,
            IsShelter = user.IsShelter,
            IsShelterVerified = user.IsShelterVerified,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match JWT expiration
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive || user.IsBanned)
        {
            throw new UnauthorizedAccessException("Your account is inactive or banned.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Update last login time (optional - we can add this field later)
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsAdmin = user.IsAdmin,
            IsShelter = user.IsShelter,
            IsShelterVerified = user.IsShelterVerified,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match JWT expiration
        };
    }

    public async Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Update user profile
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.City = request.City;
        user.Address = request.Address;
        user.ProfilePictureUrl = request.ProfilePictureUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Generate new token (optional - for updated user info)
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsAdmin = user.IsAdmin,
            IsShelter = user.IsShelter,
            IsShelterVerified = user.IsShelterVerified,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<string> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return "Password changed successfully.";
    }
}

