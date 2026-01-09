using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace PetAdoptionPlatform.Tests.TestUtilities;

public static class SeedHelper
{
    public static User CreateUser(string email, bool isAdmin = false, bool isShelter = false, bool isActive = true)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "HASH",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "000",
            City = "Istanbul",
            IsAdmin = isAdmin,
            IsShelter = isShelter,
            IsActive = isActive
        };
    }

    public static PetListing CreateListing(Guid ownerId, ListingType type = ListingType.Adoption, bool isApproved = true, bool isPaused = false)
    {
        return new PetListing
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Type = type,
            Title = "Listing",
            Description = "Desc",
            City = "Istanbul",
            District = "Kadikoy",
            IsApproved = isApproved,
            IsPaused = isPaused,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static AdoptionEligibilityForm CreateEligibilityForm(Guid userId)
    {
        return new AdoptionEligibilityForm
        {
            UserId = userId,
            LivingType = "Apartment",
            HasGarden = false,
            HasBalcony = false,
            SquareMeters = 80,
            HasPreviousPetExperience = false,
            WorkSchedule = "Full-time",
            HoursAwayFromHome = 8,
            HouseholdMembers = 2,
            HasChildren = false,
            CanAffordPetExpenses = true,
            MonthlyBudgetForPet = 1000m,
            AdditionalNotes = "Seeded form"
        };
    }

    public static AdoptionApplication CreateApplication(Guid listingId, Guid applicantId, ApplicationStatus status = ApplicationStatus.Pending)
    {
        return new AdoptionApplication
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            AdopterId = applicantId,
            Status = status,
            Message = "Hello",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static async Task SaveAsync(ApplicationDbContext ctx, params object[] entities)
    {
        foreach (var e in entities) ctx.Add(e);
        await ctx.SaveChangesAsync();
    }
}
