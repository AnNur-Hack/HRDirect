using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface ILMSService
{
    Task<ApiResponse> CreateCourseAsync(
        CreateCourseRequest request,
        int employeeId);

    Task<ApiResponse> AddLessonAsync(
        int courseId,
        CreateLessonRequest request);

    Task<ApiResponse> AddQuestionAsync(
        int courseId,
        CreateQuestionRequest request);

    Task<ApiResponse> PublishCourseAsync(int courseId);

    Task<ApiResponse> GetPublishedCoursesAsync();

    Task<ApiResponse> AssignCourseAsync(AssignCourseRequest request);

    Task<ApiResponse> GetMyAssignmentsAsync(int employeeId);

    Task<ApiResponse> CompleteLessonAsync(
        int assignmentId,
        CompleteLessonRequest request,
        int employeeId);

    Task<ApiResponse> SubmitAssessmentAsync(
        int assignmentId,
        SubmitAssessmentRequest request,
        int employeeId);

    Task<ApiResponse> ResetAssignmentAsync(
        ResetAssignmentRequest request,
        int hrEmployeeId);
}