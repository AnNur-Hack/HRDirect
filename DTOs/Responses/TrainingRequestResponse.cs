namespace HRDirectAPI.DTOs.Responses;

public class TrainingRequestResponse
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

    public string Status { get; set; }

    public int EmployeeId { get; set; }

    public int? AssignedManagerId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }
}