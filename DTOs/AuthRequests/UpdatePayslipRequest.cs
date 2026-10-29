namespace HRDirectAPI.DTOs.AuthRequests;

public class UpdatePayslipRequest
{
    public decimal BasicSalary { get; set; }

    public List<CreatePayslipItemRequest> Items { get; set; } = new();
}