namespace HRDirectAPI.DTOs.AuthRequests;

public class CorrectPayslipRequest
{
    public string Reason { get; set; } 

    public decimal BasicSalary { get; set; }

    public List<CreatePayslipItemRequest> Items { get; set; } = new();
}