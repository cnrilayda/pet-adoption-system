using PetAdoptionPlatform.Application.DTOs.EligibilityForms;

namespace PetAdoptionPlatform.Application.Features.EligibilityForms;

public interface IEligibilityFormService
{
    Task<EligibilityFormDto> CreateFormAsync(CreateEligibilityFormDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<EligibilityFormDto> UpdateFormAsync(UpdateEligibilityFormDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<EligibilityFormDto> GetFormByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EligibilityFormDto> GetFormByIdAsync(Guid formId, Guid requestingUserId, CancellationToken cancellationToken = default);
    Task<bool> HasFormAsync(Guid userId, CancellationToken cancellationToken = default);
}

