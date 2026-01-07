using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Ratings;
using PetAdoptionPlatform.Application.Features.Ratings;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RatingDto> CreateRatingAsync(CreateRatingDto request, Guid raterId, CancellationToken cancellationToken = default)
    {
        // Verify rater exists
        var rater = await _context.Users.FindAsync(new object[] { raterId }, cancellationToken);
        if (rater == null || !rater.IsActive)
        {
            throw new UnauthorizedAccessException("Rater not found or inactive.");
        }

        // Verify application exists and is completed
        var application = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        if (application.Status != ApplicationStatus.Completed)
        {
            throw new InvalidOperationException("You can only rate completed adoptions.");
        }

        // Determine who is being rated (the other party in the adoption)
        Guid ratedUserId;
        if (application.AdopterId == raterId)
        {
            // Adopter is rating the owner/shelter
            ratedUserId = application.Listing.OwnerId;
        }
        else if (application.Listing.OwnerId == raterId)
        {
            // Owner is rating the adopter
            ratedUserId = application.AdopterId;
        }
        else
        {
            throw new UnauthorizedAccessException("You can only rate applications you are involved in.");
        }

        // Check if rating already exists for this application from this rater
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.ApplicationId == request.ApplicationId && r.RaterId == raterId, cancellationToken);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Score = request.Score;
            existingRating.Comment = request.Comment;
            existingRating.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return await GetRatingByIdAsync(existingRating.Id, cancellationToken);
        }

        // Create new rating
        var rating = new Rating
        {
            RaterId = raterId,
            RatedUserId = ratedUserId,
            ApplicationId = request.ApplicationId,
            Score = request.Score,
            Comment = request.Comment
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetRatingByIdAsync(rating.Id, cancellationToken);
    }

    public async Task<RatingDto> GetRatingByIdAsync(Guid ratingId, CancellationToken cancellationToken = default)
    {
        var rating = await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .FirstOrDefaultAsync(r => r.Id == ratingId, cancellationToken);

        if (rating == null)
        {
            throw new KeyNotFoundException("Rating not found.");
        }

        return MapToDto(rating);
    }

    public async Task<IEnumerable<RatingListDto>> GetRatingsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ratings = await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .Where(r => r.RatedUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return ratings.Select(r => new RatingListDto
        {
            Id = r.Id,
            RaterId = r.RaterId,
            RaterName = $"{r.Rater.FirstName} {r.Rater.LastName}",
            RatedUserId = r.RatedUserId,
            RatedUserName = $"{r.RatedUser.FirstName} {r.RatedUser.LastName}",
            Score = r.Score,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<UserRatingSummaryDto> GetUserRatingSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ratings = await _context.Ratings
            .Where(r => r.RatedUserId == userId)
            .ToListAsync(cancellationToken);

        if (!ratings.Any())
        {
            return new UserRatingSummaryDto
            {
                UserId = userId,
                AverageRating = 0,
                TotalRatings = 0
            };
        }

        var summary = new UserRatingSummaryDto
        {
            UserId = userId,
            AverageRating = ratings.Average(r => r.Score),
            TotalRatings = ratings.Count,
            Rating1 = ratings.Count(r => r.Score == 1),
            Rating2 = ratings.Count(r => r.Score == 2),
            Rating3 = ratings.Count(r => r.Score == 3),
            Rating4 = ratings.Count(r => r.Score == 4),
            Rating5 = ratings.Count(r => r.Score == 5)
        };

        return summary;
    }

    public async Task<RatingDto?> GetRatingByApplicationAsync(Guid applicationId, Guid raterId, CancellationToken cancellationToken = default)
    {
        var rating = await _context.Ratings
            .Include(r => r.Rater)
            .Include(r => r.RatedUser)
            .FirstOrDefaultAsync(r => r.ApplicationId == applicationId && r.RaterId == raterId, cancellationToken);

        if (rating == null)
        {
            return null;
        }

        return MapToDto(rating);
    }

    private RatingDto MapToDto(Rating rating)
    {
        return new RatingDto
        {
            Id = rating.Id,
            RaterId = rating.RaterId,
            RaterName = $"{rating.Rater.FirstName} {rating.Rater.LastName}",
            RatedUserId = rating.RatedUserId,
            RatedUserName = $"{rating.RatedUser.FirstName} {rating.RatedUser.LastName}",
            ApplicationId = rating.ApplicationId,
            Score = rating.Score,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };
    }
}

