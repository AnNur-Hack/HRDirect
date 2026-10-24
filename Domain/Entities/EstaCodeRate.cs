namespace HRDirectAPI.Domain.Entities;

public class EstaCodeRate
{
    public int Id { get; set; }

    public string CountryCode { get; set; }

    public string Grade { get; set; }

    public decimal DailyRateUsd { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}