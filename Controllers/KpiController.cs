using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class KpiController(IKpiService kpiService) : ControllerBase
{
    private readonly IKpiService _kpiService = kpiService;
    
     private int CurrentEmployeeId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("plans")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> CreateKpiPlan(
        CreateKpiPlanRequest request)
    {
        var response = await _kpiService.CreateKpiPlanAsync(
            request,
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("plans/{planId:int}/items")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> AddKpiItem(
        int planId,
        CreateKpiItemRequest request)
    {
        var response = await _kpiService.AddKpiItemAsync(
            planId,
            request);

        return Ok(response);
    }

    [HttpPost("plans/{planId:int}/activate")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> ActivateKpiPlan(
        int planId,
        ActivateKpiPlanRequest request)
    {
        var response = await _kpiService.ActivateKpiPlanAsync(
            planId,
            request);

        return Ok(response);
    }

    public async Task<IActionResult> GetMyKpiPlans()
    {
        var response = await _kpiService.GetMyKpiPlansAsync(
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPut("plans/{planId:int}/progress")]
    public async Task<IActionResult> UpdateKpiProgress(
        int planId,
        UpdateKpiProgressRequest request)
    {
        var response = await _kpiService.UpdateKpiProgressAsync(
            planId,
            request,
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("plans/{planId:int}/submit")]
    public async Task<IActionResult> SubmitKpiPlan(int planId)
    {
        var response = await _kpiService.SubmitKpiPlanAsync(
            planId,
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("plans/{planId:int}/review")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> ReviewKpiPlan(
        int planId,
        ReviewKpiPlanRequest request)
    {
        var response = await _kpiService.ReviewKpiPlanAsync(
            planId,
            request,
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("plans/{planId:int}/reopen")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> ReopenKpiPlan(
        int planId,
        ReopenKpiPlanRequest request)
    {
        var response = await _kpiService.ReopenKpiPlanAsync(
            planId,
            request,
            CurrentEmployeeId);

        return Ok(response);
    }
}