using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Configurations;

public class PetListingConfiguration : IEntityTypeConfiguration<PetListing>
{
    public void Configure(EntityTypeBuilder<PetListing> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Description)
            .HasMaxLength(2000);
        
        builder.Property(p => p.RequiredAmount)
            .HasPrecision(18, 2);
        
        builder.Property(p => p.CollectedAmount)
            .HasPrecision(18, 2);
        
        // Relationships
        builder.HasMany(p => p.Applications)
            .WithOne(a => a.Listing)
            .HasForeignKey(a => a.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.Photos)
            .WithOne(ph => ph.Listing)
            .HasForeignKey(ph => ph.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(p => p.Complaints)
            .WithOne(c => c.TargetListing)
            .HasForeignKey(c => c.TargetListingId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.Favorites)
            .WithOne(f => f.Listing)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

