using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Ratings;
using PetAdoptionPlatform.Application.Features.Ratings;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly IValidator<CreateRatingDto> _createValidator;

    public RatingsController(
        IRatingService ratingService,
        IValidator<CreateRatingDto> createValidator)
    {
        _ratingService = ratingService;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RatingDto>> CreateRating(
        [FromBody] CreateRatingDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var raterId = GetCurrentUserId();
            var rating = await _ratingService.CreateRatingAsync(request, raterId, cancellationToken);
            return CreatedAtAction(nameof(GetRating), new { id = rating.Id }, rating);
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
            return StatusCode(500, new { message = "An error occurred while creating the rating.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RatingDto>> GetRating(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var rating = await _ratingService.GetRatingByIdAsync(id, cancellationToken);
            return Ok(rating);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the rating.", error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<RatingListDto>>> GetRatingsByUser(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var ratings = await _ratingService.GetRatingsByUserAsync(userId, cancellationToken);
            return Ok(ratings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching ratings.", error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/summary")]
    public async Task<ActionResult<UserRatingSummaryDto>> GetUserRatingSummary(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _ratingService.GetUserRatingSummaryAsync(userId, cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching rating summary.", error = ex.Message });
        }
    }

    [HttpGet("application/{applicationId}")]
    [Authorize]
    public async Task<ActionResult<RatingDto>> GetRatingByApplication(Guid applicationId, CancellationToken cancellationToken)
    {
        try
        {
            var raterId = GetCurrentUserId();
            var rating = await _ratingService.GetRatingByApplicationAsync(applicationId, raterId, cancellationToken);
            if (rating == null)
            {
                return NotFound(new { message = "Rating not found for this application." });
            }
            return Ok(rating);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the rating.", error = ex.Message });
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

