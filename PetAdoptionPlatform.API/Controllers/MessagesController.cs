using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetAdoptionPlatform.Application.DTOs.Messages;
using PetAdoptionPlatform.Application.Features.Messages;
using System.Security.Claims;

namespace PetAdoptionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IValidator<CreateMessageDto> _createValidator;

    public MessagesController(
        IMessageService messageService,
        IValidator<CreateMessageDto> createValidator)
    {
        _messageService = messageService;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage(
        [FromBody] CreateMessageDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var senderId = GetCurrentUserId();
            var message = await _messageService.SendMessageAsync(request, senderId, cancellationToken);
            return CreatedAtAction(nameof(GetConversation), new { applicationId = request.ApplicationId }, message);
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
            return StatusCode(500, new { message = "An error occurred while sending the message.", error = ex.Message });
        }
    }

    [HttpGet("conversation/{applicationId}")]
    public async Task<ActionResult<IEnumerable<MessageListDto>>> GetConversation(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var messages = await _messageService.GetConversationAsync(applicationId, userId, cancellationToken);
            return Ok(messages);
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
            return StatusCode(500, new { message = "An error occurred while fetching the conversation.", error = ex.Message });
        }
    }

    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<MessageListDto>>> GetUnreadMessages(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var messages = await _messageService.GetUnreadMessagesAsync(userId, cancellationToken);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching unread messages.", error = ex.Message });
        }
    }

    [HttpGet("unread/count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _messageService.GetUnreadCountAsync(userId, cancellationToken);
            return Ok(new { unreadCount = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching unread count.", error = ex.Message });
        }
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkAsReadDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _messageService.MarkMessagesAsReadAsync(request.MessageIds, userId, cancellationToken);
            return Ok(new { message = "Messages marked as read successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking messages as read.", error = ex.Message });
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

