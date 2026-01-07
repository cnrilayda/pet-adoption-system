namespace PetAdoptionPlatform.Application.DTOs.Admin;

public class AdminReportsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int BannedUsers { get; set; }
    public int ShelterUsers { get; set; }
    public int VerifiedShelters { get; set; }
    
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int PendingApprovalListings { get; set; }
    public int AdoptionListings { get; set; }
    public int LostListings { get; set; }
    public int HelpRequestListings { get; set; }
    
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int CompletedAdoptions { get; set; }
    
    public int TotalDonations { get; set; }
    public decimal TotalDonationAmount { get; set; }
    
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    
    public int PendingStories { get; set; }
    public int ApprovedStories { get; set; }
}

