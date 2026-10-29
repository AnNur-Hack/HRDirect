using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class KpiPlan
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int CreatedByEmployeeId { get; set; }

    public int? ReviewerEmployeeId { get; set; }

    public string ReviewPeriod { get; set; } 

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime DeadlineUtc { get; set; }

    public KpiPlanStatus Status { get; set; } = KpiPlanStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ActivatedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? FinalizedAtUtc { get; set; }

    public ICollection<KpiItem> Kpis { get; set; } =
        new List<KpiItem>();
    
    public ICollection<KpiReviewHistory> ReviewHistory { get; set; } =
        new List<KpiReviewHistory>();

    public ICollection<KpiPlanReopenHistory> ReopenHistory { get; set; } =
        new List<KpiPlanReopenHistory>();
}