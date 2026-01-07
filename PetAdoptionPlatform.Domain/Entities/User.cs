namespace PetAdoptionPlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Role flags
    public bool IsAdmin { get; set; } = false;
    public bool IsShelter { get; set; } = false;
    public bool IsShelterVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<PetListing> PetListings { get; set; } = new List<PetListing>();
    public virtual ICollection<AdoptionApplication> SubmittedApplications { get; set; } = new List<AdoptionApplication>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    public virtual ICollection<Complaint> SubmittedComplaints { get; set; } = new List<Complaint>();
    public virtual ICollection<Complaint> ComplaintsAgainst { get; set; } = new List<Complaint>();
    public virtual ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
    public virtual ICollection<Rating> ReceivedRatings { get; set; } = new List<Rating>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual AdoptionEligibilityForm? EligibilityForm { get; set; }
}

