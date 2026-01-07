using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Favorites;
using PetAdoptionPlatform.Application.Features.Favorites;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;

    public FavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FavoriteDto> AddFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify listing exists
        var listing = await _context.PetListings
            .Include(l => l.Photos.Where(p => p.IsPrimary))
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        // Check if already favorited
        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ListingId == listingId && f.UserId == userId, cancellationToken);

        if (existingFavorite != null)
        {
            throw new InvalidOperationException("Listing is already in favorites.");
        }

        // Create favorite
        var favorite = new Favorite
        {
            UserId = userId,
            ListingId = listingId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync(cancellationToken);

        return new FavoriteDto
        {
            Id = favorite.Id,
            ListingId = listing.Id,
            ListingTitle = listing.Title,
            ListingSpecies = listing.Species,
            ListingBreed = listing.Breed,
            PrimaryPhotoUrl = listing.Photos.FirstOrDefault()?.PhotoUrl,
            CreatedAt = favorite.CreatedAt
        };
    }

    public async Task RemoveFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ListingId == listingId && f.UserId == userId, cancellationToken);

        if (favorite == null)
        {
            throw new KeyNotFoundException("Favorite not found.");
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var favorites = await _context.Favorites
            .Include(f => f.Listing)
                .ThenInclude(l => l.Photos.Where(p => p.IsPrimary))
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        return favorites.Select(f => new FavoriteDto
        {
            Id = f.Id,
            ListingId = f.ListingId,
            ListingTitle = f.Listing.Title,
            ListingSpecies = f.Listing.Species,
            ListingBreed = f.Listing.Breed,
            PrimaryPhotoUrl = f.Listing.Photos.FirstOrDefault()?.PhotoUrl,
            CreatedAt = f.CreatedAt
        });
    }

    public async Task<bool> IsFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Favorites
            .AnyAsync(f => f.ListingId == listingId && f.UserId == userId, cancellationToken);
    }
}

