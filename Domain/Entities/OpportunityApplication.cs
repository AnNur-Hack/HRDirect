using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class OpportunityApplication
{
    public int Id { get; set; }

    public int InternalOpportunityId { get; set; }

    public InternalOpportunity InternalOpportunity { get; set; } = null!;

    public int EmployeeId { get; set; }

    public string SupportingStatement { get; set; } = string.Empty;

    public string? CvFilePath { get; set; }

    public string? CvFileName { get; set; }

    public OpportunityApplicationStatus Status { get; set; } =
        OpportunityApplicationStatus.Submitted;

    public string DepartmentAtApplication { get; set; } = string.Empty;

    public string GradeAtApplication { get; set; } = string.Empty;

    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? WithdrawnAtUtc { get; set; }

    public ICollection<OpportunityApplicationHistory> History { get; set; } =
        new List<OpportunityApplicationHistory>();
}