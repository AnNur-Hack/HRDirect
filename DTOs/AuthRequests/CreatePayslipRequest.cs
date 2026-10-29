namespace HRDirectAPI.DTOs.AuthRequests;

public class CreatePayslipRequest
{
    public int EmployeeId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal BasicSalary { get; set; }

    public List<CreatePayslipItemRequest> Items { get; set; } = new();
}