namespace HRDirectAPI.DTOs.AuthRequests;

public class UpdateKpiProgressRequest
{
    public int KpiItemId { get; set; }

    public decimal Actual { get; set; }

    public string? EvidenceComment { get; set; }
}