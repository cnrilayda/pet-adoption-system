using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;
using PetAdoptionPlatform.Application.Features.EligibilityForms;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EligibilityFormsController : ControllerBase
{
    private readonly IEligibilityFormService _formService;
    private readonly IValidator<CreateEligibilityFormDto> _createValidator;
    private readonly IValidator<UpdateEligibilityFormDto> _updateValidator;

    public EligibilityFormsController(
        IEligibilityFormService formService,
        IValidator<CreateEligibilityFormDto> createValidator,
        IValidator<UpdateEligibilityFormDto> updateValidator)
    {
        _formService = formService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<EligibilityFormDto>> CreateForm(
        [FromBody] CreateEligibilityFormDto request,
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
            var form = await _formService.CreateFormAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetMyForm), new { }, form);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the form.", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<EligibilityFormDto>> UpdateForm(
        [FromBody] UpdateEligibilityFormDto request,
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
            var form = await _formService.UpdateFormAsync(request, userId, cancellationToken);
            return Ok(form);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the form.", error = ex.Message });
        }
    }

    [HttpGet("my-form")]
    public async Task<ActionResult<EligibilityFormDto>> GetMyForm(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var form = await _formService.GetFormByUserIdAsync(userId, cancellationToken);
            return Ok(form);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the form.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EligibilityFormDto>> GetFormById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestingUserId = GetCurrentUserId();
            var form = await _formService.GetFormByIdAsync(id, requestingUserId, cancellationToken);
            return Ok(form);
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
            return StatusCode(500, new { message = "An error occurred while fetching the form.", error = ex.Message });
        }
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckHasForm(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var hasForm = await _formService.HasFormAsync(userId, cancellationToken);
            return Ok(new { hasForm });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while checking form status.", error = ex.Message });
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

