using PetAdoptionPlatform.Application.DTOs.Messages;

namespace PetAdoptionPlatform.Application.Features.Messages;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(CreateMessageDto request, Guid senderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageListDto>> GetConversationAsync(Guid applicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageListDto>> GetUnreadMessagesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkMessagesAsReadAsync(List<Guid> messageIds, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}

