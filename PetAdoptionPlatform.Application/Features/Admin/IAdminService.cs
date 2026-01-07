using PetAdoptionPlatform.Application.DTOs.Admin;

namespace PetAdoptionPlatform.Application.Features.Admin;

public interface IAdminService
{
    // Listing Management
    Task ApproveListingAsync(Guid listingId, ApproveListingDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetPendingListingsAsync(CancellationToken cancellationToken = default);
    
    // User Management
    Task<IEnumerable<UserManagementDto>> GetUsersAsync(UserFilterDto filter, CancellationToken cancellationToken = default);
    Task<UserManagementDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto request, CancellationToken cancellationToken = default);
    
    // Reports
    Task<AdminReportsDto> GetReportsAsync(CancellationToken cancellationToken = default);
}

