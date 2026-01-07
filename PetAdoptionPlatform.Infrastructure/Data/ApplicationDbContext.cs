using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Domain.Entities;

namespace PetAdoptionPlatform.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<PetListing> PetListings { get; set; }
    public DbSet<AdoptionApplication> AdoptionApplications { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<PetPhoto> PetPhotos { get; set; }
    public DbSet<AdoptionEligibilityForm> AdoptionEligibilityForms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<PetListing>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<AdoptionApplication>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Message>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<Donation>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Story>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Complaint>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Rating>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Favorite>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<PetPhoto>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<AdoptionEligibilityForm>().HasQueryFilter(e => !e.IsDeleted);
    }
}

