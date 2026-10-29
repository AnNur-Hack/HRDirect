namespace HRDirectAPI.DTOs.Responses;

public class KpiPlanResponse
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string ReviewPeriod { get; set; } 

    public string Status { get; set; } 

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime DeadlineUtc { get; set; }

    public int? ReviewerEmployeeId { get; set; }

    public decimal? OverallScore { get; set; }
}