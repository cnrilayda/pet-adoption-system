using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Stories;
using PetAdoptionPlatform.Application.Features.Stories;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;
    private readonly IValidator<CreateStoryDto> _createValidator;

    public StoriesController(
        IStoryService storyService,
        IValidator<CreateStoryDto> createValidator)
    {
        _storyService = storyService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoryListDto>>> GetApprovedStories(CancellationToken cancellationToken)
    {
        try
        {
            var stories = await _storyService.GetApprovedStoriesAsync(cancellationToken);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching stories.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoryDto>> GetStory(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var story = await _storyService.GetStoryByIdAsync(id, cancellationToken);
            return Ok(story);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the story.", error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<StoryDto>> CreateStory(
        [FromBody] CreateStoryDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var authorId = GetCurrentUserId();
            var story = await _storyService.CreateStoryAsync(request, authorId, cancellationToken);
            return CreatedAtAction(nameof(GetStory), new { id = story.Id }, story);
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
            return StatusCode(500, new { message = "An error occurred while creating the story.", error = ex.Message });
        }
    }

    [HttpGet("my-stories")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<StoryListDto>>> GetMyStories(CancellationToken cancellationToken)
    {
        try
        {
            var authorId = GetCurrentUserId();
            var stories = await _storyService.GetMyStoriesAsync(authorId, cancellationToken);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching your stories.", error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StoryDto>> ApproveStory(
        Guid id,
        [FromBody] ApproveStoryDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var story = await _storyService.ApproveStoryAsync(id, request, cancellationToken);
            return Ok(story);
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
            return StatusCode(500, new { message = "An error occurred while approving the story.", error = ex.Message });
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

