namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateKpiPlanRequest
{
    public int EmployeeId { get; set; }

    public string ReviewPeriod { get; set; }

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime DeadlineUtc { get; set; }
}