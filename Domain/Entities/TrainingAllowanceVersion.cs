namespace HRDirectAPI.Domain.Entities;

public class TrainingAllowanceVersion
{
    public int Id { get; set; }

    public int TrainingRequestId { get; set; }

    public TrainingRequest TrainingRequest { get; set; }

    public int VersionNumber { get; set; }

    public string Grade { get; set; }

    public string CountryCode { get; set; }

    public decimal DailyRateUsd { get; set; }

    public string Currency { get; set; } = "USD";

    public int EligibleDays { get; set; }

    public decimal TotalAllowance { get; set; }

    public string? CorrectionReason { get; set; }

    public int CalculatedByEmployeeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}