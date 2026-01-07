using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Applications;
using PetAdoptionPlatform.Application.Features.Applications;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IValidator<CreateApplicationDto> _createValidator;
    private readonly IValidator<UpdateApplicationStatusDto> _updateStatusValidator;

    public ApplicationsController(
        IApplicationService applicationService,
        IValidator<CreateApplicationDto> createValidator,
        IValidator<UpdateApplicationStatusDto> updateStatusValidator)
    {
        _applicationService = applicationService;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> CreateApplication(
        [FromBody] CreateApplicationDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var adopterId = GetCurrentUserId();
            var application = await _applicationService.CreateApplicationAsync(request, adopterId, cancellationToken);
            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
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
            return StatusCode(500, new { message = "An error occurred while creating the application.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDto>> GetApplication(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var application = await _applicationService.GetApplicationByIdAsync(id, userId, cancellationToken);
            return Ok(application);
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
            return StatusCode(500, new { message = "An error occurred while fetching the application.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationListDto>>> GetApplications(
        [FromQuery] ApplicationFilterDto filter,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var applications = await _applicationService.GetApplicationsAsync(filter, userId, cancellationToken);
            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching applications.", error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApplicationDto>> UpdateApplicationStatus(
        Guid id,
        [FromBody] UpdateApplicationStatusDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var userId = GetCurrentUserId();
            var application = await _applicationService.UpdateApplicationStatusAsync(id, request, userId, cancellationToken);
            return Ok(application);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the application status.", error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelApplication(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var adopterId = GetCurrentUserId();
            await _applicationService.CancelApplicationAsync(id, adopterId, cancellationToken);
            return Ok(new { message = "Application cancelled successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while cancelling the application.", error = ex.Message });
        }
    }

    [HttpGet("my-applications")]
    public async Task<ActionResult<IEnumerable<ApplicationListDto>>> GetMyApplications(CancellationToken cancellationToken)
    {
        try
        {
            var adopterId = GetCurrentUserId();
            var applications = await _applicationService.GetMyApplicationsAsync(adopterId, cancellationToken);
            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your applications.", error = ex.Message });
        }
    }

    [HttpGet("listing/{listingId}")]
    public async Task<ActionResult<IEnumerable<ApplicationListDto>>> GetListingApplications(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetCurrentUserId();
            var applications = await _applicationService.GetListingApplicationsAsync(listingId, ownerId, cancellationToken);
            return Ok(applications);
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
            return StatusCode(500, new { message = "An error occurred while fetching listing applications.", error = ex.Message });
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

