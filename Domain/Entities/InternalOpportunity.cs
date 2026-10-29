using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class InternalOpportunity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public OpportunityType Type { get; set; }

    public string Department { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string EligibilityText { get; set; } = string.Empty;

    public List<string> AllowedDepartments { get; set; } = new();

    public List<string> AllowedGrades { get; set; } = new();

    public DateTime ClosingAtUtc { get; set; }

    public OpportunityStatus Status { get; set; } = OpportunityStatus.Draft;

    public int CreatedByEmployeeId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAtUtc { get; set; }

    public ICollection<OpportunityApplication> Applications { get; set; } =
        new List<OpportunityApplication>();
}