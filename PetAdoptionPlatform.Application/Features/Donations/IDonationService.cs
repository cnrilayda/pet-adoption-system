using PetAdoptionPlatform.Application.DTOs.Donations;

namespace PetAdoptionPlatform.Application.Features.Donations;

public interface IDonationService
{
    Task<DonationDto> CreateDonationAsync(CreateDonationDto request, Guid donorId, CancellationToken cancellationToken = default);
    Task<DonationDto> GetDonationByIdAsync(Guid donationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DonationListDto>> GetDonationsAsync(Guid? listingId, Guid? donorId, CancellationToken cancellationToken = default);
    Task<DonationSummaryDto> GetDonationSummaryAsync(Guid? listingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DonationListDto>> GetMyDonationsAsync(Guid donorId, CancellationToken cancellationToken = default);
}

