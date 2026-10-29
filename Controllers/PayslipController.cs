using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PayslipController(IPayslipService payslipService) : ControllerBase
{
    private readonly IPayslipService _payslipService = payslipService;
    
   private int GetEmployeeId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private bool IsFinance()
    {
        return User.IsInRole("Finance");
    }

    [HttpPost]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> CreatePayslip(CreatePayslipRequest request)
    {
        var response = await _payslipService.CreatePayslipAsync(
            request, GetEmployeeId());

        if (response.ResponseMessage.Contains("already exists"))
        {
            return Conflict(response);
        }

        return Ok(response);
    }

    [HttpPut("{payslipId:int}")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> UpdatePayslip(
        int payslipId,
        UpdatePayslipRequest request)
    {
        var response = await _payslipService.UpdatePayslipAsync(
            payslipId, request, GetEmployeeId());

        return Ok(response);
    }

    [HttpPost("publish/{payslipId:int}")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> PublishPayslip(int payslipId)
    {
        var response = await _payslipService.PublishPayslipAsync(
            payslipId, GetEmployeeId());

        return Ok(response);
    }

    [HttpPost("corrections/{payslipId:int}")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> CreateCorrection(
        int payslipId,
        CorrectPayslipRequest request)
    {
        var response = await _payslipService.CreateCorrectionAsync(
            payslipId, request, GetEmployeeId());

        if (response.ResponseMessage.Contains("already exists"))
        {
            return Conflict(response);
        }

        return Ok(response);
    }

    [HttpGet("mypayslip")]
    public async Task<IActionResult> GetMyPayslips(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var response = await _payslipService.GetMyPayslipsAsync(
            GetEmployeeId(), month, year);

        return Ok(response);
    }

    [HttpGet("download{payslipId:int}")]
    public async Task<IActionResult> DownloadPayslip(int payslipId)
    {
        var result = await _payslipService.GetPayslipPdfAsync(
            payslipId, GetEmployeeId(), IsFinance());

        if (result.FileBytes == null)
        {
            if (result.ErrorMessage == "Payslip not found.")
            {
                return NotFound(result.ErrorMessage);
            }

            if (result.ErrorMessage ==
                "You are not allowed to download this payslip.")
            {
                return StatusCode(403, result.ErrorMessage);
            }

            return BadRequest(result.ErrorMessage);
        }

        return File(
            result.FileBytes,
            "application/pdf",
            result.FileName!);
    }
}
