namespace PetAdoptionPlatform.Application.DTOs.Messages;

public class MarkAsReadDto
{
    public List<Guid> MessageIds { get; set; } = new();
}

