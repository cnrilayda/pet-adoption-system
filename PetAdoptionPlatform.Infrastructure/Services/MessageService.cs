using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Messages;
using PetAdoptionPlatform.Application.Features.Messages;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MessageDto> SendMessageAsync(CreateMessageDto request, Guid senderId, CancellationToken cancellationToken = default)
    {
        // Verify application exists and user is part of it
        var application = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .Include(a => a.Adopter)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        // Verify sender is either Adopter or PetOwner
        var isAdopter = application.AdopterId == senderId;
        var isPetOwner = application.Listing.OwnerId == senderId;

        if (!isAdopter && !isPetOwner)
        {
            throw new UnauthorizedAccessException("You can only send messages for applications you are involved in.");
        }

        // Determine receiver
        var receiverId = isAdopter ? application.Listing.OwnerId : application.AdopterId;

        // Create message
        var message = new Message
        {
            ApplicationId = request.ApplicationId,
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = request.Content,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        // Load sender and receiver for response
        var sender = await _context.Users.FindAsync(new object[] { senderId }, cancellationToken);
        var receiver = await _context.Users.FindAsync(new object[] { receiverId }, cancellationToken);

        return new MessageDto
        {
            Id = message.Id,
            ApplicationId = message.ApplicationId,
            SenderId = message.SenderId,
            SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : string.Empty,
            ReceiverId = message.ReceiverId,
            ReceiverName = receiver != null ? $"{receiver.FirstName} {receiver.LastName}" : string.Empty,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<IEnumerable<MessageListDto>> GetConversationAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify application exists and user is part of it
        var application = await _context.AdoptionApplications
            .Include(a => a.Listing)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        var isAdopter = application.AdopterId == userId;
        var isPetOwner = application.Listing.OwnerId == userId;

        if (!isAdopter && !isPetOwner)
        {
            throw new UnauthorizedAccessException("You can only view messages for applications you are involved in.");
        }

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ApplicationId == applicationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new MessageListDto
        {
            Id = m.Id,
            ApplicationId = m.ApplicationId,
            SenderId = m.SenderId,
            SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
            ReceiverId = m.ReceiverId,
            ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<IEnumerable<MessageListDto>> GetUnreadMessagesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Application)
                .ThenInclude(a => a.Listing)
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new MessageListDto
        {
            Id = m.Id,
            ApplicationId = m.ApplicationId,
            SenderId = m.SenderId,
            SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
            ReceiverId = m.ReceiverId,
            ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task MarkMessagesAsReadAsync(List<Guid> messageIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id) && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead, cancellationToken);
    }
}

