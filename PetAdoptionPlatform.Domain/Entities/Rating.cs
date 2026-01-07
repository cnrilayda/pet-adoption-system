namespace PetAdoptionPlatform.Domain.Entities;

public class Rating : BaseEntity
{
    public Guid RaterId { get; set; }
    public Guid RatedUserId { get; set; }
    public Guid ApplicationId { get; set; } // Link to completed adoption
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
    
    // Navigation properties
    public virtual User Rater { get; set; } = null!;
    public virtual User RatedUser { get; set; } = null!;
    public virtual AdoptionApplication Application { get; set; } = null!;
}

