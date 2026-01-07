using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Admin;
using PetAdoptionPlatform.Application.Features.Admin;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("listings/{listingId}/approve")]
    public async Task<IActionResult> ApproveListing(
        Guid listingId,
        [FromBody] ApproveListingDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.ApproveListingAsync(listingId, request, cancellationToken);
            return Ok(new { message = $"Listing {(request.IsApproved ? "approved" : "rejected")} successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing the listing approval.", error = ex.Message });
        }
    }

    [HttpGet("listings/pending")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingListings(CancellationToken cancellationToken)
    {
        try
        {
            var listings = await _adminService.GetPendingListingsAsync(cancellationToken);
            return Ok(listings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching pending listings.", error = ex.Message });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsers(
        [FromQuery] UserFilterDto filter,
        CancellationToken cancellationToken)
    {
        try
        {
            var users = await _adminService.GetUsersAsync(filter, cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserManagementDto>> GetUserById(
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminService.GetUserByIdAsync(userId, cancellationToken);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching user.", error = ex.Message });
        }
    }

    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid userId,
        [FromBody] UpdateUserStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.UpdateUserStatusAsync(userId, request, cancellationToken);
            return Ok(new { message = "User status updated successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating user status.", error = ex.Message });
        }
    }

    [HttpGet("reports")]
    public async Task<ActionResult<AdminReportsDto>> GetReports(CancellationToken cancellationToken)
    {
        try
        {
            var reports = await _adminService.GetReportsAsync(cancellationToken);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching reports.", error = ex.Message });
        }
    }
}

