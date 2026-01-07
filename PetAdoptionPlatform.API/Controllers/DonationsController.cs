using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Donations;
using PetAdoptionPlatform.Application.Features.Donations;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;
    private readonly IValidator<CreateDonationDto> _createValidator;

    public DonationsController(
        IDonationService donationService,
        IValidator<CreateDonationDto> createValidator)
    {
        _donationService = donationService;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DonationDto>> CreateDonation(
        [FromBody] CreateDonationDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var donorId = GetCurrentUserId();
            var donation = await _donationService.CreateDonationAsync(request, donorId, cancellationToken);
            return CreatedAtAction(nameof(GetDonation), new { id = donation.Id }, donation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the donation.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DonationDto>> GetDonation(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var donation = await _donationService.GetDonationByIdAsync(id, cancellationToken);
            return Ok(donation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the donation.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DonationListDto>>> GetDonations(
        [FromQuery] Guid? listingId,
        [FromQuery] Guid? donorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var donations = await _donationService.GetDonationsAsync(listingId, donorId, cancellationToken);
            return Ok(donations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching donations.", error = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DonationSummaryDto>> GetDonationSummary(
        [FromQuery] Guid? listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _donationService.GetDonationSummaryAsync(listingId, cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching donation summary.", error = ex.Message });
        }
    }

    [HttpGet("my-donations")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DonationListDto>>> GetMyDonations(CancellationToken cancellationToken)
    {
        try
        {
            var donorId = GetCurrentUserId();
            var donations = await _donationService.GetMyDonationsAsync(donorId, cancellationToken);
            return Ok(donations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your donations.", error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token.");
        }
        return userId;
    }
}

