namespace PetAdoptionPlatform.Domain.Entities;

public class AdoptionEligibilityForm : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Living conditions
    public string? LivingType { get; set; } // House, Apartment, etc.
    public bool? HasGarden { get; set; }
    public bool? HasBalcony { get; set; }
    public int? SquareMeters { get; set; }
    
    // Experience
    public bool? HasPreviousPetExperience { get; set; }
    public string? PreviousPetTypes { get; set; }
    public int? YearsOfExperience { get; set; }
    
    // Household
    public int? HouseholdMembers { get; set; }
    public bool? HasChildren { get; set; }
    public int? ChildrenAge { get; set; }
    public bool? AllMembersAgree { get; set; }
    
    // Work & Time
    public string? WorkSchedule { get; set; } // Full-time, Part-time, etc.
    public int? HoursAwayFromHome { get; set; }
    public bool? CanSpendTimeWithPet { get; set; }
    
    // Financial
    public bool? CanAffordPetExpenses { get; set; }
    public decimal? MonthlyBudgetForPet { get; set; }
    
    // Additional info
    public string? AdditionalNotes { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}

