using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class TrainingRequest
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Provider { get; set; }

    public string CountryCode { get; set; }

    public string City { get; set; }

    public DateTime TrainingStartDate { get; set; }

    public DateTime TrainingEndDate { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime ReturnDate { get; set; }

    public string Justification { get; set; }

    public decimal CourseFee { get; set; }

    public string CourseFeeCurrency { get; set; }

    public decimal EstimatedFlightCost { get; set; }

    public string FlightCostCurrency { get; set; }

    public TrainingRequestStatus Status { get; set; }
        = TrainingRequestStatus.Draft;

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; }

    public int? AssignedManagerId { get; set; }

    public Employee? AssignedManager { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime? ManagerDecisionAtUtc { get; set; }

    public string? ManagerComment { get; set; }

    public DateTime? HRDecisionAtUtc { get; set; }

    public DateTime? FinanceDecisionAtUtc { get; set; }

    public string? FinanceComment { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public DateTime? WithdrawnAtUtc { get; set; }
    
    public string? CompletionEvidencePath { get; set; }

    public string? CompletionEvidenceFileName { get; set; }

    public string? CompletionEvidenceContentType { get; set; }

    public string? CompletionEvidenceComment { get; set; }

    public DateTime? CompletionEvidenceSubmittedAtUtc { get; set; }

    public int? CompletionEvidenceSubmittedByEmployeeId { get; set; }

    public string? CompletionDecisionComment { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<TrainingAllowanceVersion> AllowanceVersions { get; set; }
        = new List<TrainingAllowanceVersion>();
}