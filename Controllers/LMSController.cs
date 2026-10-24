using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LMSController(ILMSService lmsService) : ControllerBase
{
    private readonly ILMSService _lmsService =  lmsService;
    
     private int CurrentEmployeeId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("courses")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
    {
        var response = await _lmsService.CreateCourseAsync(
            request, CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("courses/{courseId:int}/lessons")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> AddLesson(
        int courseId, CreateLessonRequest request)
    {
        var response = await _lmsService.AddLessonAsync(courseId, request);
        return Ok(response);
    }

    [HttpPost("courses/{courseId:int}/questions")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> AddQuestion(
        int courseId, CreateQuestionRequest request)
    {
        var response = await _lmsService.AddQuestionAsync(courseId, request);
        return Ok(response);
    }

    [HttpPost("courses/{courseId:int}/publish")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> PublishCourse(int courseId)
    {
        var response = await _lmsService.PublishCourseAsync(courseId);
        return Ok(response);
    }

    [HttpGet("courses/published")]
    public async Task<IActionResult> GetPublishedCourses()
    {
        var response = await _lmsService.GetPublishedCoursesAsync();
        return Ok(response);
    }

    [HttpPost("assignments")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> AssignCourse(AssignCourseRequest request)
    {
        var response = await _lmsService.AssignCourseAsync(request);
        return Ok(response);
    }

    [HttpGet("assignments/my")]
    [Authorize(Roles = "Staff,Manager,HR,Finance,Admin")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var response = await _lmsService.GetMyAssignmentsAsync(
            CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("assignments/{assignmentId:int}/lessons/complete")]
    public async Task<IActionResult> CompleteLesson(
        int assignmentId, CompleteLessonRequest request)
    {
        var response = await _lmsService.CompleteLessonAsync(
            assignmentId, request, CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("assignments/{assignmentId:int}/assessment")]
    public async Task<IActionResult> SubmitAssessment(
        int assignmentId, SubmitAssessmentRequest request)
    {
        var response = await _lmsService.SubmitAssessmentAsync(
            assignmentId, request, CurrentEmployeeId);

        return Ok(response);
    }

    [HttpPost("assignments/reset")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> ResetAssignment(
        ResetAssignmentRequest request)
    {
        var response = await _lmsService.ResetAssignmentAsync(
            request, CurrentEmployeeId);

        return Ok(response);
    }
}