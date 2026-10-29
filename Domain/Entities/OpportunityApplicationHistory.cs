namespace HRDirectAPI.Domain.Entities;

public class OpportunityApplicationHistory
{
    public int Id { get; set; }

    public int OpportunityApplicationId { get; set; }

    public OpportunityApplication OpportunityApplication { get; set; }
        = null!;

    public string PreviousStatus { get; set; }

    public string NewStatus { get; set; } 

    public int ChangedByEmployeeId { get; set; }

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}