using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.DTOs.AuthRequests;

public class CreatePayslipItemRequest
{
    
    public string Name { get; set; } 

    public PayslipItemType Type { get; set; }

    public decimal Amount { get; set; }
}