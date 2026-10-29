using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class PayslipItem
{
    public int Id { get; set; }

    public int PayslipId { get; set; }

    public Payslip Payslip { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public PayslipItemType Type { get; set; }

    public decimal Amount { get; set; }
}