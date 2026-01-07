using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Donations;
using PetAdoptionPlatform.Application.Features.Donations;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class DonationService : IDonationService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;

    public DonationService(ApplicationDbContext context, IPaymentGateway paymentGateway)
    {
        _context = context;
        _paymentGateway = paymentGateway;
    }

    public async Task<DonationDto> CreateDonationAsync(CreateDonationDto request, Guid donorId, CancellationToken cancellationToken = default)
    {
        // Verify donor exists
        var donor = await _context.Users.FindAsync(new object[] { donorId }, cancellationToken);
        if (donor == null || !donor.IsActive)
        {
            throw new UnauthorizedAccessException("Donor not found or inactive.");
        }

        // If listing-specific donation, verify listing exists
        PetListing? listing = null;
        if (request.ListingId.HasValue)
        {
            listing = await _context.PetListings
                .FirstOrDefaultAsync(l => l.Id == request.ListingId.Value, cancellationToken);

            if (listing == null)
            {
                throw new KeyNotFoundException("Listing not found.");
            }

            if (listing.Type != Domain.Enums.ListingType.HelpRequest)
            {
                throw new InvalidOperationException("Donations can only be made to help request listings.");
            }
        }

        // Process payment through mocked gateway
        var paymentDescription = listing != null 
            ? $"Donation for {listing.Title}" 
            : "General shelter donation";

        var paymentResult = await _paymentGateway.ProcessPaymentAsync(request.Amount, paymentDescription, cancellationToken);

        if (!paymentResult.Success)
        {
            throw new InvalidOperationException($"Payment failed: {paymentResult.ErrorMessage}");
        }

        // Create donation record
        var donation = new Donation
        {
            DonorId = donorId,
            ListingId = request.ListingId,
            Amount = request.Amount,
            Message = request.Message,
            IsAnonymous = request.IsAnonymous,
            PaymentTransactionId = paymentResult.TransactionId,
            PaymentStatus = paymentResult.Status
        };

        _context.Donations.Add(donation);

        // Update listing collected amount if applicable
        if (listing != null)
        {
            listing.CollectedAmount = (listing.CollectedAmount ?? 0) + request.Amount;
            listing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load related data for response
        await _context.Entry(donation)
            .Reference(d => d.Donor)
            .LoadAsync(cancellationToken);

        if (donation.ListingId.HasValue)
        {
            await _context.Entry(donation)
                .Reference(d => d.Listing)
                .LoadAsync(cancellationToken);
        }

        return MapToDto(donation);
    }

    public async Task<DonationDto> GetDonationByIdAsync(Guid donationId, CancellationToken cancellationToken = default)
    {
        var donation = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Listing)
            .FirstOrDefaultAsync(d => d.Id == donationId, cancellationToken);

        if (donation == null)
        {
            throw new KeyNotFoundException("Donation not found.");
        }

        return MapToDto(donation);
    }

    public async Task<IEnumerable<DonationListDto>> GetDonationsAsync(Guid? listingId, Guid? donorId, CancellationToken cancellationToken = default)
    {
        var query = _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Listing)
            .AsQueryable();

        if (listingId.HasValue)
        {
            query = query.Where(d => d.ListingId == listingId.Value);
        }

        if (donorId.HasValue)
        {
            query = query.Where(d => d.DonorId == donorId.Value);
        }

        query = query.OrderByDescending(d => d.CreatedAt);

        var donations = await query.ToListAsync(cancellationToken);

        return donations.Select(d => new DonationListDto
        {
            Id = d.Id,
            DonorId = d.DonorId,
            DonorName = d.IsAnonymous ? "Anonymous" : $"{d.Donor.FirstName} {d.Donor.LastName}",
            ListingId = d.ListingId,
            ListingTitle = d.Listing?.Title,
            Amount = d.Amount,
            IsAnonymous = d.IsAnonymous,
            CreatedAt = d.CreatedAt
        });
    }

    public async Task<DonationSummaryDto> GetDonationSummaryAsync(Guid? listingId, CancellationToken cancellationToken = default)
    {
        var query = _context.Donations
            .Where(d => d.PaymentStatus == "Completed")
            .AsQueryable();

        if (listingId.HasValue)
        {
            query = query.Where(d => d.ListingId == listingId.Value);
        }

        var totalDonations = await query.SumAsync(d => d.Amount, cancellationToken);
        var donationCount = await query.CountAsync(cancellationToken);

        return new DonationSummaryDto
        {
            TotalDonations = totalDonations,
            DonationCount = donationCount,
            ListingTotalDonations = listingId.HasValue ? totalDonations : null,
            ListingDonationCount = listingId.HasValue ? donationCount : null
        };
    }

    public async Task<IEnumerable<DonationListDto>> GetMyDonationsAsync(Guid donorId, CancellationToken cancellationToken = default)
    {
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.Listing)
            .Where(d => d.DonorId == donorId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return donations.Select(d => new DonationListDto
        {
            Id = d.Id,
            DonorId = d.DonorId,
            DonorName = d.IsAnonymous ? "Anonymous" : $"{d.Donor.FirstName} {d.Donor.LastName}",
            ListingId = d.ListingId,
            ListingTitle = d.Listing?.Title,
            Amount = d.Amount,
            IsAnonymous = d.IsAnonymous,
            CreatedAt = d.CreatedAt
        });
    }

    private DonationDto MapToDto(Donation donation)
    {
        return new DonationDto
        {
            Id = donation.Id,
            DonorId = donation.DonorId,
            DonorName = donation.IsAnonymous ? "Anonymous" : $"{donation.Donor.FirstName} {donation.Donor.LastName}",
            ListingId = donation.ListingId,
            ListingTitle = donation.Listing?.Title,
            Amount = donation.Amount,
            Message = donation.Message,
            IsAnonymous = donation.IsAnonymous,
            PaymentTransactionId = donation.PaymentTransactionId,
            PaymentStatus = donation.PaymentStatus,
            CreatedAt = donation.CreatedAt
        };
    }
}

