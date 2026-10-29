using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InternalOpportunityController(IInternalOpportunityService opportunityService,
    IWebHostEnvironment environment) : ControllerBase
{
      private readonly IInternalOpportunityService _opportunityService =
        opportunityService;

    private readonly IWebHostEnvironment _environment = environment;

    private int CurrentEmployeeId
    {
        get
        {
            var employeeId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("id");

            return int.TryParse(employeeId, out var id) ? id : 0;
        }
    }

    private bool IsHr => User.IsInRole("HR");

    private static readonly long MaxCvSize = 5 * 1024 * 1024;

    [HttpPost]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> CreateOpportunity(
        CreateOpportunityRequest request)
    {
        if (CurrentEmployeeId <= 0)
            return Unauthorized();

        var response = await _opportunityService.CreateOpportunityAsync(
            request,
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPut("{opportunityId:int}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> UpdateOpportunity(int opportunityId,
        UpdateOpportunityRequest request)
    {
        var response = await _opportunityService.UpdateOpportunityAsync(
            opportunityId,
            request);

        return Ok(response);
    }

    [HttpPatch("{opportunityId:int}/publish")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> PublishOpportunity(int opportunityId)
    {
        var response = await _opportunityService.PublishOpportunityAsync(
            opportunityId);

        return Ok(response);
    }

    [HttpPatch("{opportunityId:int}/close")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> CloseOpportunity(int opportunityId)
    {
        var response = await _opportunityService.CloseOpportunityAsync(
            opportunityId);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetOpportunities()
    {
        var response = await _opportunityService.GetOpportunitiesAsync(IsHr);

        return Ok(response);
    }

    [HttpPost("{opportunityId:int}/apply")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ApplyForOpportunity(
        int opportunityId,
        [FromForm] string supportingStatement,
        [FromForm] IFormFile? cv)
    {
        if (CurrentEmployeeId <= 0)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(supportingStatement))
        {
            return BadRequest(
                ApiResponse.ResponseHelper.FailureResponse(
                    "Supporting statement is required."));
        }

        string? cvFilePath = null;
        string? cvFileName = null;

        if (cv != null && cv.Length > 0)
        {
            if (cv.Length > MaxCvSize)
            {
                return BadRequest(
                    ApiResponse.ResponseHelper.FailureResponse(
                        "CV must not exceed 5 MB."));
            }

            if (!string.Equals(
                    Path.GetExtension(cv.FileName),
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    ApiResponse.ResponseHelper.FailureResponse(
                        "Only PDF CV files are allowed."));
            }

            if (!string.Equals(
                    cv.ContentType,
                    "application/pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    ApiResponse.ResponseHelper.FailureResponse(
                        "The uploaded CV must be a PDF file."));
            }

            var uploadFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                "OpportunityCVs");

            Directory.CreateDirectory(uploadFolder);

            var storedFileName = $"{Guid.NewGuid()}.pdf";
            var fullPath = Path.Combine(uploadFolder, storedFileName);

            await using (var stream = new FileStream(
                             fullPath,
                             FileMode.CreateNew))
            {
                await cv.CopyToAsync(stream);
            }

            cvFilePath = fullPath;
            cvFileName = Path.GetFileName(cv.FileName);
        }

        var request = new ApplyForOpportunityRequest
        {
            SupportingStatement = supportingStatement
        };

        var response = await _opportunityService.ApplyForOpportunityAsync(
            opportunityId,
            request,
            CurrentEmployeeId,
            cvFilePath,
            cvFileName);

        if (response.ResponseMessage.Contains(
                "already applied",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        if (response.ResponseMessage.Contains(
                "not eligible",
                StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
        {
            if (cvFilePath != null && System.IO.File.Exists(cvFilePath))
                System.IO.File.Delete(cvFilePath);

            return NotFound(response);
        }

        if (response.ResponseCode != "00")
        {
            if (cvFilePath != null && System.IO.File.Exists(cvFilePath))
                System.IO.File.Delete(cvFilePath);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        if (CurrentEmployeeId <= 0)
            return Unauthorized();

        var response = await _opportunityService.GetMyApplicationsAsync(
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpGet("{opportunityId:int}/applicants")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> GetApplicants(int opportunityId)
    {
        var response = await _opportunityService.GetApplicantsAsync(
            opportunityId);

        if (response.ResponseMessage.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPatch("applications/{applicationId:int}/status")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> UpdateApplicationStatus(
        int applicationId,
        UpdateApplicationStatusRequest request)
    {
        if (CurrentEmployeeId <= 0)
            return Unauthorized();

        var response =
            await _opportunityService.UpdateApplicationStatusAsync(
                applicationId,
                request,
                CurrentEmployeeId);

        if (response.ResponseMessage.Contains(
                "Invalid status transition",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        if (response.ResponseMessage.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseCode != "00")
            return BadRequest(response);

        return Ok(response);
    }

    // Employees: Withdraw their own applications
    [HttpPatch("applications/{applicationId:int}/withdraw")]
    public async Task<IActionResult> WithdrawApplication(int applicationId)
    {
        if (CurrentEmployeeId <= 0)
            return Unauthorized();

        var response = await _opportunityService.WithdrawApplicationAsync(
            applicationId,
            CurrentEmployeeId);

        if (response.ResponseMessage.Contains(
                "no longer be withdrawn",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        if (response.ResponseMessage.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        return response.ResponseCode == "00"
            ? Ok(response)
            : BadRequest(response);
    }
}