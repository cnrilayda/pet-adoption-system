using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(d => d.PaymentTransactionId)
            .HasMaxLength(200);
        
        builder.Property(d => d.PaymentStatus)
            .HasMaxLength(50);
    }
}

