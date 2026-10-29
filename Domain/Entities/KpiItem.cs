namespace HRDirectAPI.Domain.Entities;

public class KpiItem
{
    public int Id { get; set; }

    public int KpiPlanId { get; set; }

    public KpiPlan KpiPlan { get; set; } = null!;

    public string Title { get; set; } 

    public string Description { get; set; } 

    public decimal Target { get; set; }

    public string Unit { get; set; } 

    public decimal Weight { get; set; }

    public decimal? Actual { get; set; }

    public string? EvidenceUrl { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}