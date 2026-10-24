using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Services.Implementations;

public class LMSService(ApplicationDbContext context) : ILMSService
{
    private readonly ApplicationDbContext _context = context;
    
    
    public async Task<ApiResponse> CreateCourseAsync(CreateCourseRequest request, int employeeId)
    {
        try
        {
            if (request.PassMark < 1 || request.PassMark > 100)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Pass mark must be between 1 and 100.");
            }

            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                PassMark = request.PassMark,
                CreatedByEmployeeId = employeeId,
                Status = CourseStatus.Draft
            };

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                "Course created successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while creating the course.");
        }
    }

    public async Task<ApiResponse> AddLessonAsync(int courseId, CreateLessonRequest request)
    {
        try
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse("Course not found.");
            }

            if (course.Status != CourseStatus.Draft)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Only draft courses can be edited.");
            }

            var lesson = new Lesson
            {
                CourseId = courseId,
                Title = request.Title,
                MaterialUrl = request.MaterialUrl,
                OrderNumber = request.OrderNumber
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                "Lesson added successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while adding the lesson.");
        }
    }

    public async Task<ApiResponse> AddQuestionAsync(int courseId, CreateQuestionRequest request)
    {
        try
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse("Course not found.");
            }

            if (course.Status != CourseStatus.Draft)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Only draft courses can be edited.");
            }

            if (request.Options.Count < 2 ||
                request.Options.Count(o => o.IsCorrect) != 1)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A question must have at least two options and exactly one correct option.");
            }

            var question = new QuizQuestion
            {
                CourseId = courseId,
                QuestionText = request.QuestionText,
                Options = request.Options.Select(option => new QuizOption
                {
                    OptionText = option.OptionText,
                    IsCorrect = option.IsCorrect
                }).ToList()
            };

            await _context.QuizQuestions.AddAsync(question);
            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                "Question added successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while adding the question.");
        }
    }

    public async Task<ApiResponse> PublishCourseAsync(int courseId)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse("Course not found.");
            }

            if (course.Status != CourseStatus.Draft)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Only draft courses can be published.");
            }

            if (!course.Lessons.Any() || !course.Questions.Any())
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A course must contain at least one lesson and one question before publishing.");
            }

            course.Status = CourseStatus.Published;
            course.PublishedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                "Course published successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while publishing the course.");
        }
    }

    public async Task<ApiResponse> GetPublishedCoursesAsync()
    {
        try
        {
            var courses = await _context.Courses
                .Where(c => c.Status == CourseStatus.Published)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.PassMark,
                    c.Version
                })
                .ToListAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                courses,
                "Published courses retrieved successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while retrieving published courses.");
        }
    }

    public async Task<ApiResponse> AssignCourseAsync(AssignCourseRequest request)
    {
         try
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId);

        if (course == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Course not found.");
        }

        if (course.Status != CourseStatus.Published)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only published courses can be assigned.");
        }

        if (request.DueDateUtc <= DateTime.UtcNow)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The due date must be in the future.");
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == request.EmployeeId);

        if (!employeeExists)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Employee not found.");
        }

        var alreadyAssigned = await _context.LearningAssignments
            .AnyAsync(a =>
                a.CourseId == request.CourseId &&
                a.EmployeeId == request.EmployeeId);

        if (alreadyAssigned)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "This course has already been assigned to the employee.");
        }

        var assignment = new LearningAssignment
        {
            CourseId = request.CourseId,
            EmployeeId = request.EmployeeId,
            DueDateUtc = request.DueDateUtc,
            AssignedAtUtc = DateTime.UtcNow,
            Status = LearningAssignmentStatus.NotStarted,
            CurrentCycle = 1
        };

        await _context.LearningAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            assignment,
            "Course assigned successfully.");
    }
    catch (Exception)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "An error occurred while assigning the course.");
    }
    }

    public async Task<ApiResponse> GetMyAssignmentsAsync(int employeeId)
    {
        try
        {
            var assignments = await _context.LearningAssignments
                .Where(a => a.EmployeeId == employeeId)
                .Include(a => a.Course)
                .Include(a => a.LessonCompletions)
                .Include(a => a.AssessmentAttempts)
                .Select(a => new
                {
                    a.Id,
                    CourseId = a.Course.Id,
                    CourseTitle = a.Course.Title,
                    a.DueDateUtc,
                    Status = a.Status.ToString(),
                    TotalLessons = a.Course.Lessons.Count,
                    CompletedLessons = a.LessonCompletions.Count,
                    AttemptsUsed = a.AssessmentAttempts
                        .Count(attempt => attempt.CycleNumber == a.CurrentCycle),
                    a.BestScore,
                    a.CompletedAtUtc,
                    IsOverdue = a.DueDateUtc < DateTime.UtcNow &&
                                a.Status != LearningAssignmentStatus.Completed
                })
                .ToListAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                assignments,
                "Your learning assignments were retrieved successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while retrieving your assignments.");
        }
    }

    public async Task<ApiResponse> CompleteLessonAsync(int assignmentId, CompleteLessonRequest request, int employeeId)
    {
        try
        {
            var assignment = await _context.LearningAssignments
                .Include(a => a.Course)
                .Include(a => a.LessonCompletions)
                .FirstOrDefaultAsync(a =>
                    a.Id == assignmentId &&
                    a.EmployeeId == employeeId);

            if (assignment == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Assignment not found.");
            }

            if (assignment.Status == LearningAssignmentStatus.Completed)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This assignment has already been completed.");
            }

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l =>
                    l.Id == request.LessonId &&
                    l.CourseId == assignment.CourseId);

            if (lesson == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Lesson not found in this assigned course.");
            }

            var alreadyCompleted = assignment.LessonCompletions
                .Any(lc => lc.LessonId == request.LessonId);

            if (alreadyCompleted)
            {
                return ApiResponse.ResponseHelper.SuccessResponse(
                    null,
                    "Lesson has already been completed.");
            }

            var completion = new LessonCompletion
            {
                LearningAssignmentId = assignment.Id,
                LessonId = lesson.Id,
                CompletedAtUtc = DateTime.UtcNow
            };

            await _context.LessonCompletions.AddAsync(completion);

            if (assignment.Status == LearningAssignmentStatus.NotStarted)
            {
                assignment.Status = LearningAssignmentStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Lesson completed successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while completing the lesson.");
        }
    }

    public async Task<ApiResponse> SubmitAssessmentAsync(int assignmentId, SubmitAssessmentRequest request, int employeeId)
    {
            try
    {
        var assignment = await _context.LearningAssignments
            .Include(a => a.Course)
                .ThenInclude(c => c.Questions)
                    .ThenInclude(q => q.Options)
            .Include(a => a.LessonCompletions)
            .Include(a => a.AssessmentAttempts)
            .FirstOrDefaultAsync(a =>
                a.Id == assignmentId &&
                a.EmployeeId == employeeId);

        if (assignment == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Assignment not found.");
        }

        if (assignment.Status == LearningAssignmentStatus.Completed)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "This assignment has already been completed.");
        }

        if (assignment.Status == LearningAssignmentStatus.AttemptsExhausted)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You have exhausted your attempts. Contact HR for a reset.");
        }

        var questions = assignment.Course.Questions.ToList();

        if (questions.Count == 0)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "This course has no assessment questions.");
        }

        var totalLessons = await _context.Lessons
            .CountAsync(l => l.CourseId == assignment.CourseId);

        if (assignment.LessonCompletions.Count < totalLessons)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Complete all lessons before taking the assessment.");
        }

        if (request.Answers == null ||
            request.Answers.Count != questions.Count)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You must submit exactly one answer for every question.");
        }

        var duplicateQuestions = request.Answers
            .GroupBy(a => a.QuestionId)
            .Any(g => g.Count() > 1);

        if (duplicateQuestions)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A question cannot be answered more than once.");
        }

        foreach (var answer in request.Answers)
        {
            var question = questions.FirstOrDefault(q =>
                q.Id == answer.QuestionId);

            if (question == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "An answer contains an invalid question.");
            }

            var optionBelongsToQuestion = question.Options
                .Any(o => o.Id == answer.SelectedOptionId);

            if (!optionBelongsToQuestion)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A selected option does not belong to its question.");
            }
        }

        var attemptsUsed = assignment.AssessmentAttempts
            .Count(a => a.CycleNumber == assignment.CurrentCycle);

        if (attemptsUsed >= 3)
        {
            assignment.Status = LearningAssignmentStatus.AttemptsExhausted;
            await _context.SaveChangesAsync();

            return ApiResponse.ResponseHelper.FailureResponse(
                "You have exhausted your attempts. Contact HR for a reset.");
        }

        var correctAnswers = 0;

        foreach (var answer in request.Answers)
        {
            var question = questions.First(q =>
                q.Id == answer.QuestionId);

            var selectedOption = question.Options.First(o =>
                o.Id == answer.SelectedOptionId);

            if (selectedOption.IsCorrect)
            {
                correctAnswers++;
            }
        }

        var score = (decimal)correctAnswers / questions.Count * 100;

        var isPassed = score >= assignment.Course.PassMark;

        var attempt = new AssessmentAttempt
        {
            LearningAssignmentId = assignment.Id,
            AttemptNumber = attemptsUsed + 1,
            CycleNumber = assignment.CurrentCycle,
            Score = score,
            IsPassed = isPassed,
            AttemptedAtUtc = DateTime.UtcNow
        };

        await _context.AssessmentAttempts.AddAsync(attempt);

        if (assignment.BestScore == null ||
            score > assignment.BestScore.Value)
        {
            assignment.BestScore = score;
        }

        if (isPassed)
        {
            assignment.Status = LearningAssignmentStatus.Completed;
            assignment.CompletedAtUtc = DateTime.UtcNow;
        }
        else if (attemptsUsed + 1 >= 3)
        {
            assignment.Status = LearningAssignmentStatus.AttemptsExhausted;
        }
        else
        {
            assignment.Status = LearningAssignmentStatus.InProgress;
        }

        await _context.SaveChangesAsync();

        var result = new AssessmentResultResponse
        {
            AssignmentId = assignment.Id,
            Score = score,
            IsPassed = isPassed,
            AttemptNumber = attemptsUsed + 1,
            AttemptsRemaining = isPassed ? 0 : Math.Max(0, 3 - (attemptsUsed + 1)),
            Status = assignment.Status.ToString()
        };

        return ApiResponse.ResponseHelper.SuccessResponse(
            result,
            isPassed
                ? "Congratulations! You passed the assessment."
                : assignment.Status == LearningAssignmentStatus.AttemptsExhausted
                    ? "You did not pass and have exhausted your attempts. Contact HR for a reset."
                    : "Assessment submitted. You may try again.");
    }
    catch (Exception)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "An error occurred while submitting the assessment.");
    }
    }

    public async Task<ApiResponse> ResetAssignmentAsync(ResetAssignmentRequest request, int hrEmployeeId)
    {
         try
         {
             var assignment = await _context.LearningAssignments
                 .FirstOrDefaultAsync(a => a.Id == request.AssignmentId);

             if (assignment == null)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Assignment not found.");
             }

             if (assignment.Status == LearningAssignmentStatus.Completed)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "A completed assignment cannot be reset.");
             }

             if (assignment.Status != LearningAssignmentStatus.AttemptsExhausted)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Only assignments with exhausted attempts can be reset.");
             }

             if (string.IsNullOrWhiteSpace(request.Reason))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "A reason is required to reset an assignment.");
             }

             var previousCycle = assignment.CurrentCycle;
             var newCycle = previousCycle + 1;

             var resetHistory = new AssignmentResetHistory
             {
                 LearningAssignmentId = assignment.Id,
                 ResetByEmployeeId = hrEmployeeId,
                 Reason = request.Reason.Trim(),
                 PreviousCycle = previousCycle,
                 NewCycle = newCycle,
                 ResetAtUtc = DateTime.UtcNow
             };

             await _context.AssignmentResetHistories.AddAsync(resetHistory);

             assignment.CurrentCycle = newCycle;
             assignment.Status = LearningAssignmentStatus.InProgress;

             await _context.SaveChangesAsync();

             return ApiResponse.ResponseHelper.SuccessResponse(
                 new
                 {
                     assignment.Id,
                     assignment.CurrentCycle,
                     Status = assignment.Status.ToString(),
                     assignment.BestScore
                 },
                 "Assignment reset successfully. Previous attempts and lesson progress have been preserved.");
         }
         catch (Exception)
         {
             return ApiResponse.ResponseHelper.FailureResponse(
                 "An error occurred while resetting the assignment.");
         }
    }
}