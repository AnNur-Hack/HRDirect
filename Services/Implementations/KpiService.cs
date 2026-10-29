using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Services.Implementations;

public class KpiService(ApplicationDbContext context) : IKpiService
{
    private readonly ApplicationDbContext _context = context;
    
    
  public async Task<ApiResponse> CreateKpiPlanAsync(
        CreateKpiPlanRequest request,
        int hrEmployeeId)
    {
        var existingPlan = await _context.KpiPlans
            .FirstOrDefaultAsync(p =>
                p.EmployeeId == request.EmployeeId &&
                p.ReviewPeriod == request.ReviewPeriod);

        if (existingPlan != null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A KPI plan already exists for this employee and review period."
                );
        }

        if (request.PeriodEndUtc < request.PeriodStartUtc)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The period end date cannot be earlier than the start date.");
        }

        if (request.DeadlineUtc < request.PeriodEndUtc)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The deadline cannot be earlier than the review period end date.");
        }

        var plan = new KpiPlan
        {
            EmployeeId = request.EmployeeId,
            CreatedByEmployeeId = hrEmployeeId,
            ReviewPeriod = request.ReviewPeriod,
            PeriodStartUtc = request.PeriodStartUtc,
            PeriodEndUtc = request.PeriodEndUtc,
            DeadlineUtc = request.DeadlineUtc,
            Status = KpiPlanStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.KpiPlans.Add(plan);
        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plan,
            "KPI plan created successfully.");
    }

    public async Task<ApiResponse> AddKpiItemAsync(
        int planId,
        CreateKpiItemRequest request)
    {
        var plan = await _context.KpiPlans
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.Status != KpiPlanStatus.Draft)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPIs can only be added to a draft plan.");
        }

        if (request.Target <= 0)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The KPI target must be greater than zero.");
        }

        if (request.Weight <= 0 || request.Weight > 100)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI weight must be greater than zero and cannot exceed 100.");
        }

        var totalWeight = plan.Kpis.Sum(k => k.Weight) + request.Weight;

        if (totalWeight > 100)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The total KPI weight cannot exceed 100%.");
        }

        var kpi = new KpiItem
        {
            KpiPlanId = planId,
            Title = request.Title,
            Description = request.Description,
            Target = request.Target,
            Unit = request.Unit,
            Weight = request.Weight
        };

        _context.KpiItems.Add(kpi);
        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            kpi,
            "KPI added successfully.");
    }

    public async Task<ApiResponse> ActivateKpiPlanAsync(
        int planId,
        ActivateKpiPlanRequest request)
    {
        var plan = await _context.KpiPlans
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.Status != KpiPlanStatus.Draft)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only draft KPI plans can be activated."
                );
        }

        if (!plan.Kpis.Any())
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Add at least one KPI before activating the plan.");
        }

        var totalWeight = plan.Kpis.Sum(k => k.Weight);

        if (totalWeight != 100)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI weights must add up to exactly 100% before activation.");
        }

        if (request.ReviewerEmployeeId.HasValue &&
            request.ReviewerEmployeeId.Value == plan.EmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The employee cannot be their own KPI reviewer.");
        }
        

        plan.ReviewerEmployeeId = request.ReviewerEmployeeId;
        plan.Status = KpiPlanStatus.Active;
        plan.ActivatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plan,
            "KPI plan activated successfully.");
    }

    public async Task<ApiResponse> GetMyKpiPlansAsync(int employeeId)
    {
        
        var plans = await _context.KpiPlans
            .Include(p => p.Kpis)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plans,
            "KPI plans retrieved successfully.");
    }

    public async Task<ApiResponse> UpdateKpiProgressAsync(int planId, UpdateKpiProgressRequest request, int employeeId)
    {
        var plan = await _context.KpiPlans
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.EmployeeId != employeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You are not allowed to update this KPI plan.");
        }

        if (plan.Status != KpiPlanStatus.Active &&
            plan.Status != KpiPlanStatus.Returned)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Progress can only be updated on active or returned plans.");
        }

        var kpi = plan.Kpis.FirstOrDefault(
            k => k.Id == request.KpiItemId);

        if (kpi == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI item not found in this plan.");
        }

        if (request.Actual < 0)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Actual achievement cannot be negative.");
        }

        kpi.Actual = request.Actual;
        kpi.EvidenceUrl = request.EvidenceComment;
        kpi.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            kpi,
            "KPI progress updated successfully.");
    }

    public async Task<ApiResponse> SubmitKpiPlanAsync(int planId, int employeeId)
    {
        var plan = await _context.KpiPlans
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.EmployeeId != employeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You are not allowed to submit this KPI plan.");
        }

        if (plan.Status != KpiPlanStatus.Active &&
            plan.Status != KpiPlanStatus.Returned)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "This KPI plan cannot be submitted in its current status.");
        }

        if (plan.Kpis.Any(k => !k.Actual.HasValue))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Update the actual achievement for every KPI before submission.");
        }

        plan.Status = KpiPlanStatus.Submitted;
        plan.SubmittedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plan,
            "KPI plan submitted for review successfully.");
    }

    public async Task<ApiResponse> ReviewKpiPlanAsync(int planId, ReviewKpiPlanRequest request, int reviewerEmployeeId)
    {
        var plan = await _context.KpiPlans
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.ReviewerEmployeeId != reviewerEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You are not the assigned reviewer for this plan.");
        }

        if (plan.Status != KpiPlanStatus.Submitted)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only submitted KPI plans can be reviewed.");
        }

        if (request.Decision != "Approve" &&
            request.Decision != "Return")
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Decision must be Approve or Return.");
        }

        if (request.Decision == "Return" &&
            string.IsNullOrWhiteSpace(request.Comment))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A comment is required when returning a KPI plan.");
        }

        plan.Status = request.Decision == "Approve"
            ? KpiPlanStatus.Finalized
            : KpiPlanStatus.Returned;

        if (request.Decision == "Approve")
        {
            plan.FinalizedAtUtc = DateTime.UtcNow;
        }

        var history = new KpiReviewHistory
        {
            KpiPlanId = plan.Id,
            ReviewerEmployeeId = reviewerEmployeeId,
            Decision = request.Decision,
            Comment = request.Comment,
            ReviewedAtUtc = DateTime.UtcNow
        };

        _context.KpiReviewHistories.Add(history);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plan,
            request.Decision == "Approve"
                ? "KPI plan approved and finalized."
                : "KPI plan returned for updates.");
    }

    public async Task<ApiResponse> ReopenKpiPlanAsync(int planId, ReopenKpiPlanRequest request, int hrEmployeeId)
    {
        var plan = await _context.KpiPlans
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "KPI plan not found.");
        }

        if (plan.Status != KpiPlanStatus.Active &&
            plan.Status != KpiPlanStatus.Returned)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only active or returned KPI plans can be reopened.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A reason is required to reopen a KPI plan.");
        }

        if (request.NewDeadlineUtc <= plan.DeadlineUtc)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The new deadline must be later than the current deadline.");
        }

        var history = new KpiPlanReopenHistory
        {
            KpiPlanId = plan.Id,
            ReopenedByEmployeeId = hrEmployeeId,
            Reason = request.Reason,
            PreviousDeadlineUtc = plan.DeadlineUtc,
            NewDeadlineUtc = request.NewDeadlineUtc,
            ReopenedAtUtc = DateTime.UtcNow
        };

        plan.DeadlineUtc = request.NewDeadlineUtc;
        plan.Status = KpiPlanStatus.Active;

        _context.KpiPlanReopenHistories.Add(history);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            plan,
            "KPI plan reopened successfully.");
    }
}