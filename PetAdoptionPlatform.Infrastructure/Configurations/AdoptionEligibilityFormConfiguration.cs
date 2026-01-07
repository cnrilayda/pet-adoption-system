using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Configurations;

public class AdoptionEligibilityFormConfiguration : IEntityTypeConfiguration<AdoptionEligibilityForm>
{
    public void Configure(EntityTypeBuilder<AdoptionEligibilityForm> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.MonthlyBudgetForPet)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.AdditionalNotes)
            .HasMaxLength(2000);
    }
}

