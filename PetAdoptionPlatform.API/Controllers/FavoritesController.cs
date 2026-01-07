using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Favorites;
using PetAdoptionPlatform.Application.Features.Favorites;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("{listingId}")]
    public async Task<ActionResult<FavoriteDto>> AddFavorite(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorite = await _favoriteService.AddFavoriteAsync(listingId, userId, cancellationToken);
            return Ok(favorite);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while adding to favorites.", error = ex.Message });
        }
    }

    [HttpDelete("{listingId}")]
    public async Task<IActionResult> RemoveFavorite(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _favoriteService.RemoveFavoriteAsync(listingId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while removing from favorites.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FavoriteDto>>> GetMyFavorites(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId, cancellationToken);
            return Ok(favorites);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching favorites.", error = ex.Message });
        }
    }

    [HttpGet("{listingId}/check")]
    public async Task<ActionResult<bool>> CheckFavorite(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isFavorite = await _favoriteService.IsFavoriteAsync(listingId, userId, cancellationToken);
            return Ok(new { isFavorite });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking favorite status.", error = ex.Message });
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

