using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Admin;
using PetAdoptionPlatform.Application.Features.Admin;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ApproveListingAsync(Guid listingId, ApproveListingDto request, CancellationToken cancellationToken = default)
    {
        var listing = await _context.PetListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        listing.IsApproved = request.IsApproved;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<object>> GetPendingListingsAsync(CancellationToken cancellationToken = default)
    {
        var listings = await _context.PetListings
            .Include(l => l.Owner)
            .Include(l => l.Photos.Where(p => p.IsPrimary))
            .Where(l => !l.IsApproved && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return listings.Select(l => new
        {
            l.Id,
            l.Title,
            l.Type,
            OwnerName = $"{l.Owner.FirstName} {l.Owner.LastName}",
            OwnerEmail = l.Owner.Email,
            l.CreatedAt,
            PrimaryPhotoUrl = l.Photos.FirstOrDefault()?.PhotoUrl
        });
    }

    public async Task<IEnumerable<UserManagementDto>> GetUsersAsync(UserFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        // Apply filters
        if (filter.IsAdmin.HasValue)
        {
            query = query.Where(u => u.IsAdmin == filter.IsAdmin.Value);
        }

        if (filter.IsShelter.HasValue)
        {
            query = query.Where(u => u.IsShelter == filter.IsShelter.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (filter.IsBanned.HasValue)
        {
            query = query.Where(u => u.IsBanned == filter.IsBanned.Value);
        }

        if (filter.IsShelterVerified.HasValue)
        {
            query = query.Where(u => u.IsShelterVerified == filter.IsShelterVerified.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm));
        }

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        query = query.Skip(skip).Take(filter.PageSize);

        // Order by newest first
        query = query.OrderByDescending(u => u.CreatedAt);

        var users = await query.ToListAsync(cancellationToken);

        var result = new List<UserManagementDto>();

        foreach (var user in users)
        {
            var listingCount = await _context.PetListings
                .CountAsync(l => l.OwnerId == user.Id, cancellationToken);

            var applicationCount = await _context.AdoptionApplications
                .CountAsync(a => a.AdopterId == user.Id, cancellationToken);

            result.Add(new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                IsAdmin = user.IsAdmin,
                IsShelter = user.IsShelter,
                IsShelterVerified = user.IsShelterVerified,
                IsActive = user.IsActive,
                IsBanned = user.IsBanned,
                CreatedAt = user.CreatedAt,
                ListingCount = listingCount,
                ApplicationCount = applicationCount
            });
        }

        return result;
    }

    public async Task<UserManagementDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var listingCount = await _context.PetListings
            .CountAsync(l => l.OwnerId == user.Id, cancellationToken);

        var applicationCount = await _context.AdoptionApplications
            .CountAsync(a => a.AdopterId == user.Id, cancellationToken);

        return new UserManagementDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            IsAdmin = user.IsAdmin,
            IsShelter = user.IsShelter,
            IsShelterVerified = user.IsShelterVerified,
            IsActive = user.IsActive,
            IsBanned = user.IsBanned,
            CreatedAt = user.CreatedAt,
            ListingCount = listingCount,
            ApplicationCount = applicationCount
        };
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        if (request.IsBanned.HasValue)
        {
            user.IsBanned = request.IsBanned.Value;
            if (request.IsBanned.Value)
            {
                user.IsActive = false; // Ban automatically deactivates
            }
        }

        if (request.IsShelterVerified.HasValue)
        {
            if (!user.IsShelter)
            {
                throw new InvalidOperationException("User must be a shelter to be verified.");
            }
            user.IsShelterVerified = request.IsShelterVerified.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminReportsDto> GetReportsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsBanned, cancellationToken);
        var bannedUsers = await _context.Users.CountAsync(u => u.IsBanned, cancellationToken);
        var shelterUsers = await _context.Users.CountAsync(u => u.IsShelter, cancellationToken);
        var verifiedShelters = await _context.Users.CountAsync(u => u.IsShelter && u.IsShelterVerified, cancellationToken);

        var totalListings = await _context.PetListings.CountAsync(cancellationToken);
        var activeListings = await _context.PetListings.CountAsync(l => l.IsActive && l.IsApproved, cancellationToken);
        var pendingApprovalListings = await _context.PetListings.CountAsync(l => !l.IsApproved && !l.IsDeleted, cancellationToken);
        var adoptionListings = await _context.PetListings.CountAsync(l => l.Type == ListingType.Adoption, cancellationToken);
        var lostListings = await _context.PetListings.CountAsync(l => l.Type == ListingType.Lost, cancellationToken);
        var helpRequestListings = await _context.PetListings.CountAsync(l => l.Type == ListingType.HelpRequest, cancellationToken);

        var totalApplications = await _context.AdoptionApplications.CountAsync(cancellationToken);
        var pendingApplications = await _context.AdoptionApplications.CountAsync(a => a.Status == ApplicationStatus.Pending, cancellationToken);
        var acceptedApplications = await _context.AdoptionApplications.CountAsync(a => a.Status == ApplicationStatus.Accepted, cancellationToken);
        var completedAdoptions = await _context.AdoptionApplications.CountAsync(a => a.Status == ApplicationStatus.Completed, cancellationToken);

        var totalDonations = await _context.Donations.CountAsync(d => d.PaymentStatus == "Completed", cancellationToken);
        var totalDonationAmount = await _context.Donations
            .Where(d => d.PaymentStatus == "Completed")
            .SumAsync(d => d.Amount, cancellationToken);

        var totalComplaints = await _context.Complaints.CountAsync(cancellationToken);
        var openComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Open, cancellationToken);
        var resolvedComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Resolved, cancellationToken);

        var pendingStories = await _context.Stories.CountAsync(s => s.Status == StoryStatus.Pending, cancellationToken);
        var approvedStories = await _context.Stories.CountAsync(s => s.Status == StoryStatus.Approved, cancellationToken);

        return new AdminReportsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            BannedUsers = bannedUsers,
            ShelterUsers = shelterUsers,
            VerifiedShelters = verifiedShelters,
            TotalListings = totalListings,
            ActiveListings = activeListings,
            PendingApprovalListings = pendingApprovalListings,
            AdoptionListings = adoptionListings,
            LostListings = lostListings,
            HelpRequestListings = helpRequestListings,
            TotalApplications = totalApplications,
            PendingApplications = pendingApplications,
            AcceptedApplications = acceptedApplications,
            CompletedAdoptions = completedAdoptions,
            TotalDonations = totalDonations,
            TotalDonationAmount = totalDonationAmount,
            TotalComplaints = totalComplaints,
            OpenComplaints = openComplaints,
            ResolvedComplaints = resolvedComplaints,
            PendingStories = pendingStories,
            ApprovedStories = approvedStories
        };
    }
}

