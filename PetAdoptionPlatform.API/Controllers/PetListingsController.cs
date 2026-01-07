using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Application.Features.Listings;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetListingsController : ControllerBase
{
    private readonly IPetListingService _listingService;
    private readonly IValidator<CreatePetListingDto> _createValidator;
    private readonly IValidator<UpdatePetListingDto> _updateValidator;

    public PetListingsController(
        IPetListingService listingService,
        IValidator<CreatePetListingDto> createValidator,
        IValidator<UpdatePetListingDto> updateValidator)
    {
        _listingService = listingService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetListingListDto>>> GetListings(
        [FromQuery] PetListingFilterDto filter,
        CancellationToken cancellationToken)
    {
        var listings = await _listingService.GetListingsAsync(filter, cancellationToken);
        return Ok(listings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PetListingDto>> GetListing(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingService.GetListingByIdAsync(id, cancellationToken);
            return Ok(listing);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PetListingDto>> CreateListing(
        [FromBody] CreatePetListingDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var userId = GetCurrentUserId();
            var listing = await _listingService.CreateListingAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the listing.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PetListingDto>> UpdateListing(
        Guid id,
        [FromBody] UpdatePetListingDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var userId = GetCurrentUserId();
            var listing = await _listingService.UpdateListingAsync(id, request, userId, cancellationToken);
            return Ok(listing);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the listing.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteListing(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _listingService.DeleteListingAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the listing.", error = ex.Message });
        }
    }

    [HttpPost("{id}/toggle-pause")]
    [Authorize]
    public async Task<IActionResult> TogglePause(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _listingService.TogglePauseListingAsync(id, userId, cancellationToken);
            return Ok(new { message = "Listing pause status toggled successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while toggling pause status.", error = ex.Message });
        }
    }

    [HttpGet("my-listings")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PetListingListDto>>> GetMyListings(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var listings = await _listingService.GetUserListingsAsync(userId, cancellationToken);
            return Ok(listings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your listings.", error = ex.Message });
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

