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
public class TrainingController(ITrainingService trainingService) : ControllerBase
{
    private readonly ITrainingService _trainingService = trainingService;
    
      private int GetEmployeeId()
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(employeeId, out var id) ? id : 0;
    }

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft(
        [FromBody] CreateTrainingRequest request)
    {
        var employeeId = GetEmployeeId();

        if (employeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.CreateDraftAsync(
            employeeId, request);

        if (response.ResponseCode != "00")
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var employeeId = GetEmployeeId();

        if (employeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.GetMyRequestsAsync(employeeId);

        if (response.ResponseCode != "00")
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("draft/{id:int}")]
    public async Task<IActionResult> UpdateDraft(
        int id,
        [FromBody] UpdateTrainingRequest request)
    {
        var employeeId = GetEmployeeId();

        if (employeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.UpdateDraftAsync(
            employeeId, id, request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains("not found",
                    StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains("not authorized",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains("Only draft",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("submit/{id:int}")]
    public async Task<IActionResult> SubmitForApproval(int id)
    {
        var employeeId = GetEmployeeId();

        if (employeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.SubmitForApprovalAsync(
            employeeId, id);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains("not found",
                    StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains("not authorized",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains("Only draft",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPost("manager-approve/{id:int}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ApproveByManager(
        int id,
        [FromBody] ManagerTrainingDecisionRequest request)
    {
        var managerId = GetEmployeeId();

        if (managerId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.ApproveByManagerAsync(
            managerId, id, request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains("not found",
                    StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains("not authorized",
                    StringComparison.OrdinalIgnoreCase) ||
                response.ResponseMessage.Contains("own training",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains("not awaiting",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPost("manager-reject/{id:int}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> RejectByManager(
        int id,
        [FromBody] ManagerTrainingDecisionRequest request)
    {
        var managerId = GetEmployeeId();

        if (managerId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.RejectByManagerAsync(
            managerId, id, request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains("not found",
                    StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains("not authorized",
                    StringComparison.OrdinalIgnoreCase) ||
                response.ResponseMessage.Contains("own training",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains("not awaiting",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPost("hr-approve/{id:int}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> ApproveByHR(
        int id,
        [FromBody] HRTrainingDecisionRequest request)
    {
        try
        {
            var hrEmployeeId = GetEmployeeId();

            if (hrEmployeeId == 0)
            {
                return Unauthorized();
            }

            var response = await _trainingService.ApproveByHRAsync(
                hrEmployeeId, id, request);

            if (response.ResponseCode != "00")
            {
                if (response.ResponseMessage.Contains(
                        "not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }

                if (response.ResponseMessage.Contains(
                        "not authorized", StringComparison.OrdinalIgnoreCase) ||
                    response.ResponseMessage.Contains(
                        "own training", StringComparison.OrdinalIgnoreCase) ||
                    response.ResponseMessage.Contains(
                        "own request", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden, response);
                }

                if (response.ResponseMessage.Contains(
                        "not awaiting", StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(response);
                }

                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    ResponseCode = "99",
                    ResponseMessage = "An error occurred while processing HR approval."
                });
        }
    }
    
    [HttpPost("hr-return/{id:int}")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> ReturnToManager(
        int id,
        [FromBody] HRTrainingDecisionRequest request)
    {
        try
        {
            var hrEmployeeId = GetEmployeeId();

            if (hrEmployeeId == 0)
            {
                return Unauthorized();
            }

            var response = await _trainingService.ReturnToManagerAsync(
                hrEmployeeId, id, request);

            if (response.ResponseCode != "00")
            {
                if (response.ResponseMessage.Contains(
                        "not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }

                if (response.ResponseMessage.Contains(
                        "not authorized", StringComparison.OrdinalIgnoreCase) ||
                    response.ResponseMessage.Contains(
                        "own training", StringComparison.OrdinalIgnoreCase) ||
                    response.ResponseMessage.Contains(
                        "own request", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden, response);
                }

                if (response.ResponseMessage.Contains(
                        "not awaiting", StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(response);
                }

                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    ResponseCode = "99",
                    ResponseMessage = "An error occurred while returning the training request."
                });
        }
    }
    
    [HttpPost("finance-approve/{id:int}")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> ApproveByFinance(
        int id,
        [FromBody] FinanceTrainingDecisionRequest request)
    {
        var financeEmployeeId = GetEmployeeId();

        if (financeEmployeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.ApproveByFinanceAsync(
            financeEmployeeId, id, request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains(
                    "not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains(
                    "own training", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains(
                    "not awaiting", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPost("finance-reject/{id:int}")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> RejectByFinance(
        int id,
        [FromBody] FinanceTrainingDecisionRequest request)
    {
        var financeEmployeeId = GetEmployeeId();

        if (financeEmployeeId == 0)
        {
            return Unauthorized();
        }

        var response = await _trainingService.RejectByFinanceAsync(
            financeEmployeeId, id, request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage.Contains(
                    "not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            if (response.ResponseMessage.Contains(
                    "own training", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }

            if (response.ResponseMessage.Contains(
                    "not awaiting", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPost("completion-evidence/{id:int}")]
[Consumes("multipart/form-data")]
[Authorize]
public async Task<IActionResult> SubmitCompletionEvidence(
    int id,
    [FromForm] TrainingCompletionEvidenceRequest request)
{
    var response = await _trainingService.SubmitCompletionEvidenceAsync(
        GetEmployeeId(),
        id,
        request);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "after the training end date", StringComparison.OrdinalIgnoreCase) ||
            response.ResponseMessage.Contains(
                "approved requests", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }

    return Ok(response);
}


[HttpPost("complete/{id:int}")]
[Authorize(Roles = "HR")]
public async Task<IActionResult> CompleteTraining(
    int id,
    [FromBody] TrainingCompletionDecisionRequest request)
{
    var response = await _trainingService.CompleteTrainingAsync(
        GetEmployeeId(),
        id,
        request);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "status", StringComparison.OrdinalIgnoreCase) ||
            response.ResponseMessage.Contains(
                "already", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }

    return Ok(response);
}


[HttpPost("completion-evidence-return/{id:int}")]
[Authorize(Roles = "HR")]
public async Task<IActionResult> ReturnCompletionEvidence(
    int id,
    [FromBody] TrainingCompletionDecisionRequest request)
{
    var response = await _trainingService.ReturnCompletionEvidenceAsync(
        GetEmployeeId(),
        id,
        request);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "status", StringComparison.OrdinalIgnoreCase) ||
            response.ResponseMessage.Contains(
                "already", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }

    return Ok(response);
}


[HttpPost("close-without-evidence/{id:int}")]
[Authorize(Roles = "HR")]
public async Task<IActionResult> CloseWithoutEvidence(
    int id,
    [FromBody] TrainingCompletionDecisionRequest request)
{
    var response = await _trainingService.CloseWithoutEvidenceAsync(
        GetEmployeeId(),
        id,
        request);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "60 days", StringComparison.OrdinalIgnoreCase) ||
            response.ResponseMessage.Contains(
                "status", StringComparison.OrdinalIgnoreCase) ||
            response.ResponseMessage.Contains(
                "already", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }

    return Ok(response);
}
[HttpGet("/attachments/{id:int}/download")]
[Authorize]
public async Task<IActionResult> DownloadCompletionEvidence(int id)
{
    var response = await _trainingService.DownloadCompletionEvidenceAsync(
        GetEmployeeId(),
        id);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        return BadRequest(response);
    }

    var download = response.Data as TrainingEvidenceDownloadResponse;

    if (download == null)
    {
        return NotFound(response);
    }

    return File(
        download.FileContent,
        download.ContentType,
        download.FileName);
}
[HttpPost("withdraw/{id:int}")]
public async Task<IActionResult> WithdrawTrainingRequest(
    int id,
    [FromBody] WithdrawTrainingRequest request)
{
    var employeeId = GetEmployeeId();

    if (employeeId == 0)
    {
        return Unauthorized();
    }

    var response = await _trainingService.WithdrawTrainingRequestAsync(
        employeeId,
        id,
        request);

    if (response.ResponseCode != "00")
    {
        if (response.ResponseMessage.Contains(
                "not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        if (response.ResponseMessage.Contains(
                "not authorized", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, response);
        }

        if (response.ResponseMessage.Contains(
                "cannot be withdrawn", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }

    return Ok(response);
}
[HttpPost("admin/reassign-reviewer/{id:int}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ReassignTrainingReviewer(
    int id,
    [FromBody] ReassignTrainingReviewerRequest request)
{
    var response = await _trainingService.ReassignTrainingReviewerAsync(
        GetEmployeeId(),
        id,
        request);

    if (response.ResponseCode != "00")
    {
        return BadRequest(response);
    }

    return Ok(response);
}

[HttpGet("admin/pending-manager/all")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetPendingManagerRequests()
{
    var response = await _trainingService.GetPendingManagerRequestsAsync(
        GetEmployeeId());

    if (response.ResponseCode != "00")
    {
        return BadRequest(response);
    }

    return Ok(response);
}
}