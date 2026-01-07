using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Score)
            .IsRequired();
        
        builder.HasCheckConstraint("CK_Rating_Score", "Score >= 1 AND Score <= 5");
        
        builder.Property(r => r.Comment)
            .HasMaxLength(1000);
    }
}

