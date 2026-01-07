using PetAdoptionPlatform.Application.DTOs.Favorites;

namespace PetAdoptionPlatform.Application.Features.Favorites;

public interface IFavoriteService
{
    Task<FavoriteDto> AddFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
}

