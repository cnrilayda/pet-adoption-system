using PetAdoptionPlatform.Application.DTOs.Listings;

namespace PetAdoptionPlatform.Application.Features.Listings;

public interface IPetListingService
{
    Task<PetListingDto> CreateListingAsync(CreatePetListingDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<PetListingDto> GetListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PetListingListDto>> GetListingsAsync(PetListingFilterDto filter, CancellationToken cancellationToken = default);
    Task<PetListingDto> UpdateListingAsync(Guid listingId, UpdatePetListingDto request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
    Task TogglePauseListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PetListingListDto>> GetUserListingsAsync(Guid userId, CancellationToken cancellationToken = default);
}

