using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateOpportunityRequest
{
    public string Title { get; set; } 

    public OpportunityType Type { get; set; }

    public string Department { get; set; } 

    public string Description { get; set; } 

    public string EligibilityText { get; set; } 

    public List<string> AllowedDepartments { get; set; } = new();

    public List<string> AllowedGrades { get; set; } = new();

    public DateTime ClosingAtUtc { get; set; }
}