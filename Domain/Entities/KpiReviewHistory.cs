namespace HRDirectAPI.Domain.Entities;

public class KpiReviewHistory
{
    public int Id { get; set; }

    public int KpiPlanId { get; set; }

    public KpiPlan KpiPlan { get; set; } = null!;

    public int ReviewerEmployeeId { get; set; }

    public string Decision { get; set; } 

    public string? Comment { get; set; }

    public DateTime ReviewedAtUtc { get; set; } = DateTime.UtcNow;
}