using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Applications;
using PetAdoptionPlatform.Application.Features.Applications;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly ApplicationDbContext _context;

    public ApplicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto request, Guid adopterId, CancellationToken cancellationToken = default)
    {
        // Check if listing exists and is active
        var listing = await _context.PetListings
            .Include(l => l.Owner)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        if (!listing.IsActive || !listing.IsApproved || listing.IsPaused)
        {
            throw new InvalidOperationException("This listing is not available for applications.");
        }

        if (listing.OwnerId == adopterId)
        {
            throw new InvalidOperationException("You cannot apply to your own listing.");
        }

        // Only Adoption listings can have applications
        if (listing.Type != ListingType.Adoption)
        {
            throw new InvalidOperationException("Applications can only be created for Adoption listings.");
        }

        // Check if user has eligibility form (required for adoption listings)
        if (listing.Type == ListingType.Adoption)
        {
            var hasEligibilityForm = await _context.AdoptionEligibilityForms
                .AnyAsync(e => e.UserId == adopterId, cancellationToken);

            if (!hasEligibilityForm)
            {
                throw new InvalidOperationException("You must complete the Adoption Eligibility Form before applying.");
            }
        }

        // Check if user already applied to this listing
        var existingApplication = await _context.AdoptionApplications
            .FirstOrDefaultAsync(a => a.ListingId == request.ListingId && a.AdopterId == adopterId, cancellationToken);

        if (existingApplication != null && existingApplication.Status != ApplicationStatus.Rejected && existingApplication.Status != ApplicationStatus.Cancelled)
        {
            throw new InvalidOperationException("You have already applied to this listing.");
        }

        // Create new application
        var application = new AdoptionApplication
        {
            ListingId = request.ListingId,
            AdopterId = adopterId,
            Status = ApplicationStatus.Pending,
            Message = request.Message
        };

        _context.AdoptionApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetApplicationByIdAsync(application.Id, adopterId, cancellationToken);
    }

    public async Task<ApplicationDto> GetApplicationByIdAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var application = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .Include(a => a.Adopter)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        // Check authorization: Adopter can see their own, PetOwner can see applications to their listings
        var isAdopter = application.AdopterId == userId;
        var isPetOwner = application.Listing.OwnerId == userId;
        var isAdmin = await _context.Users
            .AnyAsync(u => u.Id == userId && u.IsAdmin, cancellationToken);

        if (!isAdopter && !isPetOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this application.");
        }

        var eligibilityForm = await _context.AdoptionEligibilityForms
            .FirstOrDefaultAsync(e => e.UserId == application.AdopterId, cancellationToken);

        return new ApplicationDto
        {
            Id = application.Id,
            ListingId = application.ListingId,
            ListingTitle = application.Listing.Title,
            AdopterId = application.AdopterId,
            AdopterName = $"{application.Adopter.FirstName} {application.Adopter.LastName}",
            AdopterEmail = application.Adopter.Email,
            AdopterPhone = application.Adopter.PhoneNumber,
            Status = application.Status,
            Message = application.Message,
            AdminNotes = application.AdminNotes,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            HasEligibilityForm = eligibilityForm != null,
            EligibilityFormId = eligibilityForm?.Id
        };
    }

    public async Task<IEnumerable<ApplicationListDto>> GetApplicationsAsync(ApplicationFilterDto filter, Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _context.AdoptionApplications
            .Include(a => a.Listing)
            .Include(a => a.Adopter)
            .AsQueryable();

        // Apply filters
        if (filter.ListingId.HasValue)
        {
            query = query.Where(a => a.ListingId == filter.ListingId.Value);
        }

        if (filter.AdopterId.HasValue)
        {
            query = query.Where(a => a.AdopterId == filter.AdopterId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        // Authorization: Users can only see their own applications or applications to their listings
        var isAdmin = await _context.Users
            .AnyAsync(u => u.Id == userId && u.IsAdmin, cancellationToken);

        if (!isAdmin)
        {
            query = query.Where(a => 
                a.AdopterId == userId || 
                a.Listing.OwnerId == userId);
        }

        // Pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        query = query.Skip(skip).Take(filter.PageSize);

        // Order by newest first
        query = query.OrderByDescending(a => a.CreatedAt);

        var applications = await query.ToListAsync(cancellationToken);

        return applications.Select(a => new ApplicationListDto
        {
            Id = a.Id,
            ListingId = a.ListingId,
            ListingTitle = a.Listing.Title,
            AdopterId = a.AdopterId,
            AdopterName = $"{a.Adopter.FirstName} {a.Adopter.LastName}",
            Status = a.Status,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task<ApplicationDto> UpdateApplicationStatusAsync(Guid applicationId, UpdateApplicationStatusDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var application = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        // Only PetOwner or Admin can update status
        var isPetOwner = application.Listing.OwnerId == userId;
        var isAdmin = await _context.Users
            .AnyAsync(u => u.Id == userId && u.IsAdmin, cancellationToken);

        if (!isPetOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this application status.");
        }

        // Validate status transition
        if (!IsValidStatusTransition(application.Status, request.Status))
        {
            throw new InvalidOperationException($"Invalid status transition from {application.Status} to {request.Status}.");
        }

        application.Status = request.Status;
        application.AdminNotes = request.AdminNotes;
        application.UpdatedAt = DateTime.UtcNow;

        // If accepted, mark other pending applications for the same listing as rejected
        if (request.Status == ApplicationStatus.Accepted)
        {
            var otherApplications = await _context.AdoptionApplications
                .Where(a => a.ListingId == application.ListingId && 
                           a.Id != applicationId && 
                           (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.UnderReview))
                .ToListAsync(cancellationToken);

            foreach (var otherApp in otherApplications)
            {
                otherApp.Status = ApplicationStatus.Rejected;
                otherApp.UpdatedAt = DateTime.UtcNow;
            }

            // Pause the listing
            application.Listing.IsPaused = true;
            application.Listing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetApplicationByIdAsync(applicationId, userId, cancellationToken);
    }

    public async Task CancelApplicationAsync(Guid applicationId, Guid adopterId, CancellationToken cancellationToken = default)
    {
        var application = await _context.AdoptionApplications
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        if (application.AdopterId != adopterId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own applications.");
        }

        if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
        {
            throw new InvalidOperationException("You can only cancel pending or under review applications.");
        }

        application.Status = ApplicationStatus.Cancelled;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationListDto>> GetMyApplicationsAsync(Guid adopterId, CancellationToken cancellationToken = default)
    {
        var applications = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .Include(a => a.Adopter)
            .Where(a => a.AdopterId == adopterId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return applications.Select(a => new ApplicationListDto
        {
            Id = a.Id,
            ListingId = a.ListingId,
            ListingTitle = a.Listing.Title,
            AdopterId = a.AdopterId,
            AdopterName = $"{a.Adopter.FirstName} {a.Adopter.LastName}",
            Status = a.Status,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task<IEnumerable<ApplicationListDto>> GetListingApplicationsAsync(Guid listingId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Verify ownership
        var listing = await _context.PetListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new KeyNotFoundException("Listing not found.");
        }

        if (listing.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You can only view applications for your own listings.");
        }

        var applications = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .Include(a => a.Adopter)
            .Where(a => a.ListingId == listingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return applications.Select(a => new ApplicationListDto
        {
            Id = a.Id,
            ListingId = a.ListingId,
            ListingTitle = a.Listing.Title,
            AdopterId = a.AdopterId,
            AdopterName = $"{a.Adopter.FirstName} {a.Adopter.LastName}",
            Status = a.Status,
            CreatedAt = a.CreatedAt
        });
    }

    private bool IsValidStatusTransition(ApplicationStatus currentStatus, ApplicationStatus newStatus)
    {
        return currentStatus switch
        {
            ApplicationStatus.Pending => newStatus == ApplicationStatus.UnderReview || 
                                        newStatus == ApplicationStatus.Rejected || 
                                        newStatus == ApplicationStatus.Cancelled,
            ApplicationStatus.UnderReview => newStatus == ApplicationStatus.Accepted || 
                                            newStatus == ApplicationStatus.Rejected || 
                                            newStatus == ApplicationStatus.Cancelled,
            ApplicationStatus.Accepted => newStatus == ApplicationStatus.Completed,
            ApplicationStatus.Rejected => false, // Cannot change from rejected
            ApplicationStatus.Completed => false, // Cannot change from completed
            ApplicationStatus.Cancelled => false, // Cannot change from cancelled
            _ => false
        };
    }
}

