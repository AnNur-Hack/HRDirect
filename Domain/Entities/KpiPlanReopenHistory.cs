namespace HRDirectAPI.Domain.Entities;

public class KpiPlanReopenHistory
{
    public int Id { get; set; }

    public int KpiPlanId { get; set; }

    public KpiPlan KpiPlan { get; set; } = null!;

    public int ReopenedByEmployeeId { get; set; }

    public string Reason { get; set; } 

    public DateTime PreviousDeadlineUtc { get; set; }

    public DateTime NewDeadlineUtc { get; set; }

    public DateTime ReopenedAtUtc { get; set; } = DateTime.UtcNow;
}