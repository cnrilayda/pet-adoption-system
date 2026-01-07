namespace PetAdoptionPlatform.Application.DTOs.Messages;

public class CreateMessageDto
{
    public Guid ApplicationId { get; set; }
    public string Content { get; set; } = string.Empty;
}

