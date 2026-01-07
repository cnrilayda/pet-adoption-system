using PetAdoptionPlatform.Application.DTOs.Applications;

namespace PetAdoptionPlatform.Application.Features.Applications;

public interface IApplicationService
{
    Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto request, Guid adopterId, CancellationToken cancellationToken = default);
    Task<ApplicationDto> GetApplicationByIdAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationListDto>> GetApplicationsAsync(ApplicationFilterDto filter, Guid userId, CancellationToken cancellationToken = default);
    Task<ApplicationDto> UpdateApplicationStatusAsync(Guid applicationId, UpdateApplicationStatusDto request, Guid userId, CancellationToken cancellationToken = default);
    Task CancelApplicationAsync(Guid applicationId, Guid adopterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationListDto>> GetMyApplicationsAsync(Guid adopterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationListDto>> GetListingApplicationsAsync(Guid listingId, Guid ownerId, CancellationToken cancellationToken = default);
}

