using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Application.Features.Listings;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class PetListingService : IPetListingService
{
    private readonly ApplicationDbContext _context;

    public PetListingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PetListingDto> CreateListingAsync(CreatePetListingDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        var listing = new PetListing
        {
            OwnerId = userId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            Species = request.Species,
            Breed = request.Breed,
            Age = request.Age,
            Gender = request.Gender,
            Size = request.Size,
            Color = request.Color,
            IsVaccinated = request.IsVaccinated,
            IsNeutered = request.IsNeutered,
            City = request.City,
            District = request.District,
            RequiredAmount = request.RequiredAmount,
            CollectedAmount = 0,
            IsActive = true,
            IsApproved = false, // Admin approval required
            IsPaused = false
        };

        _context.PetListings.Add(listing);

        // Add photos if provided
        if (request.PhotoUrls != null && request.PhotoUrls.Any())
        {
            var photos = request.PhotoUrls.Select((url, index) => new PetPhoto
            {
                ListingId = listing.Id,
                PhotoUrl = url,
                IsPrimary = index == 0,
                DisplayOrder = index
            }).ToList();

            _context.PetPhotos.AddRange(photos);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetListingByIdAsync(listing.Id, cancellationToken);
    }

    public async Task<PetListingDto> GetListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.PetListings
            .Include(l => l.Owner)
            .Include(l => l.Photos.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        var applicationCount = await _context.AdoptionApplications
            .CountAsync(a => a.ListingId == listingId, cancellationToken);

        var favoriteCount = await _context.Favorites
            .CountAsync(f => f.ListingId == listingId, cancellationToken);

        return MapToDto(listing, applicationCount, favoriteCount);
    }

    public async Task<IEnumerable<PetListingListDto>> GetListingsAsync(PetListingFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.PetListings
            .Include(l => l.Owner)
            .Include(l => l.Photos.OrderBy(p => p.DisplayOrder))
            .AsQueryable();

        // Apply filters
        if (filter.Type.HasValue)
        {
            query = query.Where(l => l.Type == filter.Type.Value);
        }

        if (!string.IsNullOrEmpty(filter.Species))
        {
            query = query.Where(l => l.Species == filter.Species);
        }

        if (!string.IsNullOrEmpty(filter.Breed))
        {
            query = query.Where(l => l.Breed == filter.Breed);
        }

        if (!string.IsNullOrEmpty(filter.Gender))
        {
            query = query.Where(l => l.Gender == filter.Gender);
        }

        if (!string.IsNullOrEmpty(filter.City))
        {
            query = query.Where(l => l.City == filter.City);
        }

        if (filter.IsShelter.HasValue)
        {
            query = query.Where(l => l.Owner.IsShelter == filter.IsShelter.Value);
        }

        if (filter.MinAge.HasValue)
        {
            query = query.Where(l => l.Age.HasValue && l.Age >= filter.MinAge.Value);
        }

        if (filter.MaxAge.HasValue)
        {
            query = query.Where(l => l.Age.HasValue && l.Age <= filter.MaxAge.Value);
        }

        if (filter.IsVaccinated.HasValue)
        {
            query = query.Where(l => l.IsVaccinated == filter.IsVaccinated.Value);
        }

        if (filter.IsNeutered.HasValue)
        {
            query = query.Where(l => l.IsNeutered == filter.IsNeutered.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(l => 
                l.Title.ToLower().Contains(searchTerm) || 
                l.Description.ToLower().Contains(searchTerm));
        }

        // Only show active and approved listings (unless admin)
        query = query.Where(l => l.IsActive && l.IsApproved);

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        query = query.Skip(skip).Take(filter.PageSize);

        // Order by newest first
        query = query.OrderByDescending(l => l.CreatedAt);

        var listings = await query.ToListAsync(cancellationToken);

        return listings.Select(l => new PetListingListDto
        {
            Id = l.Id,
            OwnerId = l.OwnerId,
            OwnerName = $"{l.Owner.FirstName} {l.Owner.LastName}",
            IsShelter = l.Owner.IsShelter,
            Type = l.Type,
            Title = l.Title,
            Description = l.Description,
            Species = l.Species,
            Breed = l.Breed,
            Age = l.Age,
            Gender = l.Gender,
            City = l.City,
            PrimaryPhotoUrl = l.Photos.FirstOrDefault()?.PhotoUrl,
            PhotoUrls = l.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).ToList(),
            IsVaccinated = l.IsVaccinated,
            IsNeutered = l.IsNeutered,
            IsActive = l.IsActive,
            IsApproved = l.IsApproved,
            RequiredAmount = l.RequiredAmount,
            CollectedAmount = l.CollectedAmount,
            CreatedAt = l.CreatedAt
        });
    }

    public async Task<PetListingDto> UpdateListingAsync(Guid listingId, UpdatePetListingDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.PetListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        if (listing.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own listings.");
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Title))
            listing.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            listing.Description = request.Description;

        if (request.Species != null)
            listing.Species = request.Species;

        if (request.Breed != null)
            listing.Breed = request.Breed;

        if (request.Age.HasValue)
            listing.Age = request.Age;

        if (request.Gender != null)
            listing.Gender = request.Gender;

        if (request.Size != null)
            listing.Size = request.Size;

        if (request.Color != null)
            listing.Color = request.Color;

        if (request.IsVaccinated.HasValue)
            listing.IsVaccinated = request.IsVaccinated;

        if (request.IsNeutered.HasValue)
            listing.IsNeutered = request.IsNeutered;

        if (request.City != null)
            listing.City = request.City;

        if (request.District != null)
            listing.District = request.District;

        if (request.IsPaused.HasValue)
            listing.IsPaused = request.IsPaused.Value;

        if (request.RequiredAmount.HasValue)
            listing.RequiredAmount = request.RequiredAmount;

        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetListingByIdAsync(listingId, cancellationToken);
    }

    public async Task DeleteListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.PetListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        if (listing.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own listings.");
        }

        // Soft delete
        listing.IsDeleted = true;
        listing.DeletedAt = DateTime.UtcNow;
        listing.IsActive = false;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task TogglePauseListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.PetListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        if (listing.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You can only pause/resume your own listings.");
        }

        listing.IsPaused = !listing.IsPaused;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<PetListingListDto>> GetUserListingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var listings = await _context.PetListings
            .Include(l => l.Owner)
            .Include(l => l.Photos.OrderBy(p => p.DisplayOrder))
            .Where(l => l.OwnerId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return listings.Select(l => new PetListingListDto
        {
            Id = l.Id,
            OwnerId = l.OwnerId,
            OwnerName = $"{l.Owner.FirstName} {l.Owner.LastName}",
            IsShelter = l.Owner.IsShelter,
            Type = l.Type,
            Title = l.Title,
            Description = l.Description,
            Species = l.Species,
            Breed = l.Breed,
            Age = l.Age,
            Gender = l.Gender,
            City = l.City,
            PrimaryPhotoUrl = l.Photos.FirstOrDefault()?.PhotoUrl,
            PhotoUrls = l.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).ToList(),
            IsVaccinated = l.IsVaccinated,
            IsNeutered = l.IsNeutered,
            IsActive = l.IsActive,
            IsApproved = l.IsApproved,
            RequiredAmount = l.RequiredAmount,
            CollectedAmount = l.CollectedAmount,
            CreatedAt = l.CreatedAt
        });
    }

    private PetListingDto MapToDto(PetListing listing, int applicationCount, int favoriteCount)
    {
        return new PetListingDto
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            OwnerName = $"{listing.Owner.FirstName} {listing.Owner.LastName}",
            IsShelter = listing.Owner.IsShelter,
            Type = listing.Type,
            Title = listing.Title,
            Description = listing.Description,
            Species = listing.Species,
            Breed = listing.Breed,
            Age = listing.Age,
            Gender = listing.Gender,
            Size = listing.Size,
            Color = listing.Color,
            IsVaccinated = listing.IsVaccinated,
            IsNeutered = listing.IsNeutered,
            City = listing.City,
            District = listing.District,
            IsActive = listing.IsActive,
            IsApproved = listing.IsApproved,
            IsPaused = listing.IsPaused,
            RequiredAmount = listing.RequiredAmount,
            CollectedAmount = listing.CollectedAmount,
            PhotoUrls = listing.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).ToList(),
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            ApplicationCount = applicationCount,
            FavoriteCount = favoriteCount
        };
    }
}

