using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;

namespace PetAdoptionPlatform.Infrastructure.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DbSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Check if admin already exists
        var adminExists = await _context.Users.AnyAsync(u => u.IsAdmin);
        if (!adminExists)
        {
            var admin = new User
            {
                Email = "admin@petadoption.com",
                PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                IsAdmin = true,
                IsActive = true,
                IsShelter = false,
                IsBanned = false,
                IsShelterVerified = false
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }

        // Seed dummy listings if they don't exist
        var listingsExist = await _context.PetListings.AnyAsync();
        if (!listingsExist)
        {
            // Get or create a test user for listings
            var testUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            if (testUser == null)
            {
                testUser = new User
                {
                    Email = "test@example.com",
                    PasswordHash = _passwordHasher.HashPassword("Test123!"),
                    FirstName = "Test",
                    LastName = "User",
                    IsAdmin = false,
                    IsActive = true,
                    IsShelter = false,
                    IsBanned = false,
                    IsShelterVerified = false,
                    City = "İstanbul"
                };
                _context.Users.Add(testUser);
                await _context.SaveChangesAsync();
            }

            // Create dummy listings
            var listings = new List<PetListing>
            {
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Adoption,
                    Title = "Sevimli Golden Retriever Yavru",
                    Description = "2 aylık sevimli Golden Retriever yavru. Aşıları tamamlanmış, çok sevecen ve oyuncu. Çocuklarla çok iyi anlaşıyor.",
                    Species = "Köpek",
                    Breed = "Golden Retriever",
                    Age = 2,
                    Gender = "Dişi",
                    Size = "Orta",
                    Color = "Altın Sarısı",
                    IsVaccinated = true,
                    IsNeutered = false,
                    City = "İstanbul",
                    District = "Kadıköy",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Adoption,
                    Title = "Sakin British Shorthair",
                    Description = "1 yaşında sakin ve uyumlu British Shorthair. Ev ortamına çok uygun, bakımı kolay.",
                    Species = "Kedi",
                    Breed = "British Shorthair",
                    Age = 12,
                    Gender = "Erkek",
                    Size = "Orta",
                    Color = "Gri",
                    IsVaccinated = true,
                    IsNeutered = true,
                    City = "Ankara",
                    District = "Çankaya",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Lost,
                    Title = "Kayıp Labrador Köpek",
                    Description = "Kayıp siyah Labrador köpek. 3 yaşında, boynunda kırmızı tasma var. Son görüldüğü yer: Kadıköy sahili.",
                    Species = "Köpek",
                    Breed = "Labrador",
                    Age = 36,
                    Gender = "Erkek",
                    Size = "Büyük",
                    Color = "Siyah",
                    City = "İstanbul",
                    District = "Kadıköy",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Adoption,
                    Title = "Oyuncu Beagle",
                    Description = "Enerjik ve oyuncu Beagle. 8 aylık, çok sevecen. Aşıları tam, kısırlaştırılmış.",
                    Species = "Köpek",
                    Breed = "Beagle",
                    Age = 8,
                    Gender = "Dişi",
                    Size = "Küçük",
                    Color = "Üç Renkli",
                    IsVaccinated = true,
                    IsNeutered = true,
                    City = "İzmir",
                    District = "Konak",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.HelpRequest,
                    Title = "Veteriner Masrafları İçin Yardım",
                    Description = "Sokakta bulduğumuz yaralı kedi için acil veteriner masraflarına ihtiyacımız var. Ameliyat gerekiyor.",
                    Species = "Kedi",
                    Breed = "Tekir",
                    Age = 24,
                    Gender = "Dişi",
                    Size = "Orta",
                    Color = "Gri-Beyaz",
                    City = "Bursa",
                    District = "Nilüfer",
                    RequiredAmount = 5000,
                    CollectedAmount = 2500,
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Adoption,
                    Title = "Sakin Persian Kedi",
                    Description = "2 yaşında sakin ve nazik Persian kedi. Uzun tüylü, bakımı düzenli yapılmalı.",
                    Species = "Kedi",
                    Breed = "Persian",
                    Age = 24,
                    Gender = "Dişi",
                    Size = "Orta",
                    Color = "Beyaz",
                    IsVaccinated = true,
                    IsNeutered = true,
                    City = "Antalya",
                    District = "Muratpaşa",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.LookingForPet,
                    Title = "Golden Retriever Yavru Arıyorum",
                    Description = "Ailemize katılmak üzere Golden Retriever yavru arıyoruz. Sorumlu sahiplenme yapacağız.",
                    Species = "Köpek",
                    Breed = "Golden Retriever",
                    Age = null,
                    Gender = null,
                    City = "İstanbul",
                    District = "Beşiktaş",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                },
                new PetListing
                {
                    OwnerId = testUser.Id,
                    Type = ListingType.Adoption,
                    Title = "Enerjik Border Collie",
                    Description = "Çok zeki ve enerjik Border Collie. 18 aylık, eğitilmiş, aktif yaşam tarzına uygun.",
                    Species = "Köpek",
                    Breed = "Border Collie",
                    Age = 18,
                    Gender = "Erkek",
                    Size = "Orta",
                    Color = "Siyah-Beyaz",
                    IsVaccinated = true,
                    IsNeutered = true,
                    City = "İstanbul",
                    District = "Şişli",
                    IsActive = true,
                    IsApproved = true,
                    IsPaused = false
                }
            };

            _context.PetListings.AddRange(listings);
            await _context.SaveChangesAsync();

            // Add photos for each listing (using leylos.jpeg)
            var photoUrl = "/src/assets/leylos.jpeg";
            foreach (var listing in listings)
            {
                var photo = new PetPhoto
                {
                    ListingId = listing.Id,
                    PhotoUrl = photoUrl,
                    IsPrimary = true,
                    DisplayOrder = 0
                };
                _context.PetPhotos.Add(photo);
            }

            await _context.SaveChangesAsync();
        }
    }
}

