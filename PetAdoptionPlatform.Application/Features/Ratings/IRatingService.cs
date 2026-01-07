using PetAdoptionPlatform.Application.DTOs.Ratings;

namespace PetAdoptionPlatform.Application.Features.Ratings;

public interface IRatingService
{
    Task<RatingDto> CreateRatingAsync(CreateRatingDto request, Guid raterId, CancellationToken cancellationToken = default);
    Task<RatingDto> GetRatingByIdAsync(Guid ratingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RatingListDto>> GetRatingsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserRatingSummaryDto> GetUserRatingSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RatingDto?> GetRatingByApplicationAsync(Guid applicationId, Guid raterId, CancellationToken cancellationToken = default);
}

