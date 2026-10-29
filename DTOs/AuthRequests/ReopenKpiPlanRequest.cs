namespace HRDirectAPI.DTOs.AuthRequests;

public class ReopenKpiPlanRequest
{
    public string Reason { get; set; } 

    public DateTime NewDeadlineUtc { get; set; }
}