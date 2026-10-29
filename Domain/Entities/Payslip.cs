using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class Payslip
{
    public int Id { get; set; }

    public string PayslipReference { get; set; } 

    public int EmployeeId { get; set; }

    public int PreparedByEmployeeId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int Version { get; set; } = 1;

    public int? ParentPayslipId { get; set; }

    public Payslip? ParentPayslip { get; set; }

    public ICollection<Payslip> Revisions { get; set; } = new List<Payslip>();

    public string? CorrectionReason { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal GrossPay { get; set; }

    public decimal NetPay { get; set; }

    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;

    public bool IsCurrent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAtUtc { get; set; }

    public ICollection<PayslipItem> Items { get; set; } = new List<PayslipItem>();
}