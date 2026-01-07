using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Complaints;
using PetAdoptionPlatform.Application.Features.Complaints;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;
    private readonly IValidator<CreateComplaintDto> _createValidator;

    public ComplaintsController(
        IComplaintService complaintService,
        IValidator<CreateComplaintDto> createValidator)
    {
        _complaintService = complaintService;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<ActionResult<ComplaintDto>> CreateComplaint(
        [FromBody] CreateComplaintDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var complainantId = GetCurrentUserId();
            var complaint = await _complaintService.CreateComplaintAsync(request, complainantId, cancellationToken);
            return CreatedAtAction(nameof(GetComplaint), new { id = complaint.Id }, complaint);
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
            return StatusCode(500, new { message = "An error occurred while creating the complaint.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComplaintDto>> GetComplaint(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var complaint = await _complaintService.GetComplaintByIdAsync(id, userId, cancellationToken);
            return Ok(complaint);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the complaint.", error = ex.Message });
        }
    }

    [HttpGet("my-complaints")]
    public async Task<ActionResult<IEnumerable<ComplaintListDto>>> GetMyComplaints(CancellationToken cancellationToken)
    {
        try
        {
            var complainantId = GetCurrentUserId();
            var complaints = await _complaintService.GetMyComplaintsAsync(complainantId, cancellationToken);
            return Ok(complaints);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your complaints.", error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ComplaintListDto>>> GetComplaints(
        [FromQuery] Guid? complainantId,
        [FromQuery] Guid? targetUserId,
        [FromQuery] Guid? targetListingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var complaints = await _complaintService.GetComplaintsAsync(complainantId, targetUserId, targetListingId, cancellationToken);
            return Ok(complaints);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching complaints.", error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ComplaintDto>> UpdateComplaintStatus(
        Guid id,
        [FromBody] UpdateComplaintStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var complaint = await _complaintService.UpdateComplaintStatusAsync(id, request, cancellationToken);
            return Ok(complaint);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the complaint status.", error = ex.Message });
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

