using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Complaints;
using PetAdoptionPlatform.Application.Features.Complaints;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class ComplaintService : IComplaintService
{
    private readonly ApplicationDbContext _context;

    public ComplaintService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto request, Guid complainantId, CancellationToken cancellationToken = default)
    {
        // Verify complainant exists
        var complainant = await _context.Users.FindAsync(new object[] { complainantId }, cancellationToken);
        if (complainant == null || !complainant.IsActive)
        {
            throw new UnauthorizedAccessException("Complainant not found or inactive.");
        }

        // Verify target exists
        if (request.TargetUserId.HasValue)
        {
            var targetUser = await _context.Users.FindAsync(new object[] { request.TargetUserId.Value }, cancellationToken);
            if (targetUser == null)
            {
                throw new KeyNotFoundException("Target user not found.");
            }

            if (request.TargetUserId.Value == complainantId)
            {
                throw new InvalidOperationException("You cannot file a complaint against yourself.");
            }
        }

        if (request.TargetListingId.HasValue)
        {
            var targetListing = await _context.PetListings.FindAsync(new object[] { request.TargetListingId.Value }, cancellationToken);
            if (targetListing == null)
            {
                throw new KeyNotFoundException("Target listing not found.");
            }
        }

        var complaint = new Complaint
        {
            ComplainantId = complainantId,
            TargetUserId = request.TargetUserId,
            TargetListingId = request.TargetListingId,
            Reason = request.Reason,
            Description = request.Description,
            Status = ComplaintStatus.Open
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetComplaintByIdAsync(complaint.Id, complainantId, cancellationToken);
    }

    public async Task<ComplaintDto> GetComplaintByIdAsync(Guid complaintId, Guid userId, CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Complainant)
            .Include(c => c.TargetUser)
            .Include(c => c.TargetListing)
            .FirstOrDefaultAsync(c => c.Id == complaintId, cancellationToken);

        if (complaint == null)
        {
            throw new KeyNotFoundException("Complaint not found.");
        }

        // Authorization: Complainant, TargetUser, or Admin can view
        var isComplainant = complaint.ComplainantId == userId;
        var isTargetUser = complaint.TargetUserId == userId;
        var isAdmin = await _context.Users
            .AnyAsync(u => u.Id == userId && u.IsAdmin, cancellationToken);

        if (!isComplainant && !isTargetUser && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this complaint.");
        }

        return MapToDto(complaint);
    }

    public async Task<IEnumerable<ComplaintListDto>> GetComplaintsAsync(Guid? complainantId, Guid? targetUserId, Guid? targetListingId, CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints
            .Include(c => c.Complainant)
            .AsQueryable();

        if (complainantId.HasValue)
        {
            query = query.Where(c => c.ComplainantId == complainantId.Value);
        }

        if (targetUserId.HasValue)
        {
            query = query.Where(c => c.TargetUserId == targetUserId.Value);
        }

        if (targetListingId.HasValue)
        {
            query = query.Where(c => c.TargetListingId == targetListingId.Value);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        var complaints = await query.ToListAsync(cancellationToken);

        return complaints.Select(c => new ComplaintListDto
        {
            Id = c.Id,
            ComplainantId = c.ComplainantId,
            ComplainantName = $"{c.Complainant.FirstName} {c.Complainant.LastName}",
            TargetUserId = c.TargetUserId,
            TargetListingId = c.TargetListingId,
            Reason = c.Reason,
            Status = c.Status,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<ComplaintDto> UpdateComplaintStatusAsync(Guid complaintId, UpdateComplaintStatusDto request, CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == complaintId, cancellationToken);

        if (complaint == null)
        {
            throw new KeyNotFoundException("Complaint not found.");
        }

        complaint.Status = request.Status;
        complaint.AdminNotes = request.AdminNotes;
        complaint.ResolutionNotes = request.ResolutionNotes;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Get admin user ID for viewing (or use a service method that bypasses auth check)
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.IsAdmin, cancellationToken);
        var adminId = adminUser?.Id ?? Guid.Empty;

        return await GetComplaintByIdAsync(complaintId, adminId, cancellationToken);
    }

    public async Task<IEnumerable<ComplaintListDto>> GetMyComplaintsAsync(Guid complainantId, CancellationToken cancellationToken = default)
    {
        var complaints = await _context.Complaints
            .Include(c => c.Complainant)
            .Where(c => c.ComplainantId == complainantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return complaints.Select(c => new ComplaintListDto
        {
            Id = c.Id,
            ComplainantId = c.ComplainantId,
            ComplainantName = $"{c.Complainant.FirstName} {c.Complainant.LastName}",
            TargetUserId = c.TargetUserId,
            TargetListingId = c.TargetListingId,
            Reason = c.Reason,
            Status = c.Status,
            CreatedAt = c.CreatedAt
        });
    }

    private ComplaintDto MapToDto(Complaint complaint)
    {
        return new ComplaintDto
        {
            Id = complaint.Id,
            ComplainantId = complaint.ComplainantId,
            ComplainantName = $"{complaint.Complainant.FirstName} {complaint.Complainant.LastName}",
            TargetUserId = complaint.TargetUserId,
            TargetUserName = complaint.TargetUser != null ? $"{complaint.TargetUser.FirstName} {complaint.TargetUser.LastName}" : null,
            TargetListingId = complaint.TargetListingId,
            TargetListingTitle = complaint.TargetListing?.Title,
            Reason = complaint.Reason,
            Description = complaint.Description,
            Status = complaint.Status,
            AdminNotes = complaint.AdminNotes,
            ResolutionNotes = complaint.ResolutionNotes,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt
        };
    }
}

