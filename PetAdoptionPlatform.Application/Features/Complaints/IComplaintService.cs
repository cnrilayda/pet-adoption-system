using PetAdoptionPlatform.Application.DTOs.Complaints;

namespace PetAdoptionPlatform.Application.Features.Complaints;

public interface IComplaintService
{
    Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto request, Guid complainantId, CancellationToken cancellationToken = default);
    Task<ComplaintDto> GetComplaintByIdAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintListDto>> GetComplaintsAsync(Guid? complainantId, Guid? targetUserId, Guid? targetListingId, CancellationToken cancellationToken = default);
    Task<ComplaintDto> UpdateComplaintStatusAsync(Guid complaintId, UpdateComplaintStatusDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintListDto>> GetMyComplaintsAsync(Guid complainantId, CancellationToken cancellationToken = default);
}

