using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        // Relationships
        builder.HasMany(u => u.PetListings)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.SubmittedApplications)
            .WithOne(a => a.Adopter)
            .HasForeignKey(a => a.AdopterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.Receiver)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.Donations)
            .WithOne(d => d.Donor)
            .HasForeignKey(d => d.DonorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.Stories)
            .WithOne(s => s.Author)
            .HasForeignKey(s => s.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.SubmittedComplaints)
            .WithOne(c => c.Complainant)
            .HasForeignKey(c => c.ComplainantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.GivenRatings)
            .WithOne(r => r.Rater)
            .HasForeignKey(r => r.RaterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.ReceivedRatings)
            .WithOne(r => r.RatedUser)
            .HasForeignKey(r => r.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.Favorites)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(u => u.EligibilityForm)
            .WithOne(e => e.User)
            .HasForeignKey<AdoptionEligibilityForm>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

