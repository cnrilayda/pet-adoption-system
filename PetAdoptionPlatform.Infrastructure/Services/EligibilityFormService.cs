using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;
using PetAdoptionPlatform.Application.Features.EligibilityForms;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class EligibilityFormService : IEligibilityFormService
{
    private readonly ApplicationDbContext _context;

    public EligibilityFormService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EligibilityFormDto> CreateFormAsync(CreateEligibilityFormDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if user already has a form
        var existingForm = await _context.AdoptionEligibilityForms
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (existingForm != null)
        {
            throw new InvalidOperationException("You already have an eligibility form. Please update the existing one.");
        }

        // Verify user exists
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var form = new AdoptionEligibilityForm
        {
            UserId = userId,
            LivingType = request.LivingType,
            HasGarden = request.HasGarden,
            HasBalcony = request.HasBalcony,
            SquareMeters = request.SquareMeters,
            HasPreviousPetExperience = request.HasPreviousPetExperience,
            PreviousPetTypes = request.PreviousPetTypes,
            YearsOfExperience = request.YearsOfExperience,
            HouseholdMembers = request.HouseholdMembers,
            HasChildren = request.HasChildren,
            ChildrenAge = request.ChildrenAge,
            AllMembersAgree = request.AllMembersAgree,
            WorkSchedule = request.WorkSchedule,
            HoursAwayFromHome = request.HoursAwayFromHome,
            CanSpendTimeWithPet = request.CanSpendTimeWithPet,
            CanAffordPetExpenses = request.CanAffordPetExpenses,
            MonthlyBudgetForPet = request.MonthlyBudgetForPet,
            AdditionalNotes = request.AdditionalNotes
        };

        _context.AdoptionEligibilityForms.Add(form);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(form);
    }

    public async Task<EligibilityFormDto> UpdateFormAsync(UpdateEligibilityFormDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var form = await _context.AdoptionEligibilityForms
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("Eligibility form not found. Please create one first.");
        }

        // Update only provided fields
        if (request.LivingType != null)
            form.LivingType = request.LivingType;

        if (request.HasGarden.HasValue)
            form.HasGarden = request.HasGarden;

        if (request.HasBalcony.HasValue)
            form.HasBalcony = request.HasBalcony;

        if (request.SquareMeters.HasValue)
            form.SquareMeters = request.SquareMeters;

        if (request.HasPreviousPetExperience.HasValue)
            form.HasPreviousPetExperience = request.HasPreviousPetExperience;

        if (request.PreviousPetTypes != null)
            form.PreviousPetTypes = request.PreviousPetTypes;

        if (request.YearsOfExperience.HasValue)
            form.YearsOfExperience = request.YearsOfExperience;

        if (request.HouseholdMembers.HasValue)
            form.HouseholdMembers = request.HouseholdMembers;

        if (request.HasChildren.HasValue)
            form.HasChildren = request.HasChildren;

        if (request.ChildrenAge.HasValue)
            form.ChildrenAge = request.ChildrenAge;

        if (request.AllMembersAgree.HasValue)
            form.AllMembersAgree = request.AllMembersAgree;

        if (request.WorkSchedule != null)
            form.WorkSchedule = request.WorkSchedule;

        if (request.HoursAwayFromHome.HasValue)
            form.HoursAwayFromHome = request.HoursAwayFromHome;

        if (request.CanSpendTimeWithPet.HasValue)
            form.CanSpendTimeWithPet = request.CanSpendTimeWithPet;

        if (request.CanAffordPetExpenses.HasValue)
            form.CanAffordPetExpenses = request.CanAffordPetExpenses;

        if (request.MonthlyBudgetForPet.HasValue)
            form.MonthlyBudgetForPet = request.MonthlyBudgetForPet;

        if (request.AdditionalNotes != null)
            form.AdditionalNotes = request.AdditionalNotes;

        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(form);
    }

    public async Task<EligibilityFormDto> GetFormByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var form = await _context.AdoptionEligibilityForms
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("Eligibility form not found.");
        }

        return MapToDto(form);
    }

    public async Task<EligibilityFormDto> GetFormByIdAsync(Guid formId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var form = await _context.AdoptionEligibilityForms
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == formId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("Eligibility form not found.");
        }

        // Only the form owner or admin can view
        var isOwner = form.UserId == requestingUserId;
        var isAdmin = await _context.Users
            .AnyAsync(u => u.Id == requestingUserId && u.IsAdmin, cancellationToken);

        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this form.");
        }

        return MapToDto(form);
    }

    public async Task<bool> HasFormAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionEligibilityForms
            .AnyAsync(e => e.UserId == userId, cancellationToken);
    }

    private EligibilityFormDto MapToDto(AdoptionEligibilityForm form)
    {
        return new EligibilityFormDto
        {
            Id = form.Id,
            UserId = form.UserId,
            LivingType = form.LivingType,
            HasGarden = form.HasGarden,
            HasBalcony = form.HasBalcony,
            SquareMeters = form.SquareMeters,
            HasPreviousPetExperience = form.HasPreviousPetExperience,
            PreviousPetTypes = form.PreviousPetTypes,
            YearsOfExperience = form.YearsOfExperience,
            HouseholdMembers = form.HouseholdMembers,
            HasChildren = form.HasChildren,
            ChildrenAge = form.ChildrenAge,
            AllMembersAgree = form.AllMembersAgree,
            WorkSchedule = form.WorkSchedule,
            HoursAwayFromHome = form.HoursAwayFromHome,
            CanSpendTimeWithPet = form.CanSpendTimeWithPet,
            CanAffordPetExpenses = form.CanAffordPetExpenses,
            MonthlyBudgetForPet = form.MonthlyBudgetForPet,
            AdditionalNotes = form.AdditionalNotes,
            CreatedAt = form.CreatedAt,
            UpdatedAt = form.UpdatedAt
        };
    }
}

