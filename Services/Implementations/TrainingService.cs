using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Repositories.Interfaces;
using HRDirectAPI.Services.Interfaces;


namespace HRDirectAPI.Services.Implementations;

public class TrainingService(ITrainingRepository trainingRepository, 
    IAuthRepository authRepository, IWebHostEnvironment environment, IEmailService emailService) : ITrainingService
{
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly IEmailService _emailService = emailService;
    
    
     public async Task<ApiResponse> CreateDraftAsync(
        int employeeId,
        CreateTrainingRequest request)
    {
        try
        {
            var employee =
                await _authRepository.GetEmployeeByIdAsync(employeeId);

            if (employee == null || !employee.IsActive)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Employee not found or inactive.");
            }

            var validationMessage = ValidateTrainingRequest(request);

            if (validationMessage != null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse(validationMessage);
            }

            var trainingRequest = new TrainingRequest
            {
                Title = request.Title.Trim(),
                Provider = request.Provider.Trim(),
                CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
                City = request.City.Trim(),
                TrainingStartDate = request.TrainingStartDate,
                TrainingEndDate = request.TrainingEndDate,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                Justification = request.Justification.Trim(),
                CourseFee = Math.Round(
                    request.CourseFee, 2, MidpointRounding.AwayFromZero),
                CourseFeeCurrency = request.CourseFeeCurrency.Trim().ToUpperInvariant(),
                EstimatedFlightCost = Math.Round(
                    request.EstimatedFlightCost, 2, MidpointRounding.AwayFromZero),
                FlightCostCurrency = request.FlightCostCurrency.Trim().ToUpperInvariant(),
                EmployeeId = employeeId,
                AssignedManagerId = employee.ReportingManagerId,
                Status = TrainingRequestStatus.Draft,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _trainingRepository.AddAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                new TrainingRequestResponse
                {
                    Id = trainingRequest.Id,
                    Title = trainingRequest.Title,
                    Provider = trainingRequest.Provider,
                    CountryCode = trainingRequest.CountryCode,
                    City = trainingRequest.City,
                    TrainingStartDate = trainingRequest.TrainingStartDate,
                    TrainingEndDate = trainingRequest.TrainingEndDate,
                    DepartureDate = trainingRequest.DepartureDate,
                    ReturnDate = trainingRequest.ReturnDate,
                    Justification = trainingRequest.Justification,
                    CourseFee = trainingRequest.CourseFee,
                    CourseFeeCurrency = trainingRequest.CourseFeeCurrency,
                    EstimatedFlightCost = trainingRequest.EstimatedFlightCost,
                    FlightCostCurrency = trainingRequest.FlightCostCurrency,
                    Status = trainingRequest.Status.ToString(),
                    EmployeeId = trainingRequest.EmployeeId,
                    AssignedManagerId = trainingRequest.AssignedManagerId,
                    CreatedAtUtc = trainingRequest.CreatedAtUtc,
                    SubmittedAtUtc = trainingRequest.SubmittedAtUtc
                },
                "Training request draft created successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Unable to create training request.");
        }
    }

    public async Task<ApiResponse> GetMyRequestsAsync(int employeeId)
    {
        var employee =
            await _authRepository.GetEmployeeByIdAsync(employeeId);

        if (employee == null || !employee.IsActive)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Employee not found or inactive.");
        }

        var requests =
            await _trainingRepository.GetByEmployeeIdAsync(employeeId);

        var response = requests.Select(t => new TrainingRequestResponse
        {
            Id = t.Id,
            Title = t.Title,
            Provider = t.Provider,
            CountryCode = t.CountryCode,
            City = t.City,
            TrainingStartDate = t.TrainingStartDate,
            TrainingEndDate = t.TrainingEndDate,
            DepartureDate = t.DepartureDate,
            ReturnDate = t.ReturnDate,
            Justification = t.Justification,
            CourseFee = t.CourseFee,
            CourseFeeCurrency = t.CourseFeeCurrency,
            EstimatedFlightCost = t.EstimatedFlightCost,
            FlightCostCurrency = t.FlightCostCurrency,
            Status = t.Status.ToString(),
            EmployeeId = t.EmployeeId,
            AssignedManagerId = t.AssignedManagerId,
            CreatedAtUtc = t.CreatedAtUtc,
            SubmittedAtUtc = t.SubmittedAtUtc
        }).ToList();

        return ApiResponse.ResponseHelper.SuccessResponse(
            response,
            "Training requests retrieved successfully.");
    }

    public async Task<ApiResponse> UpdateDraftAsync(
        int employeeId,
        int requestId,
        UpdateTrainingRequest request)
    {
        var trainingRequest =
            await _trainingRepository.GetByIdAsync(requestId);

        if (trainingRequest == null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Training request not found.");
        }

        if (trainingRequest.EmployeeId != employeeId)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("You are not authorized to update this request.");
        }

        if (trainingRequest.Status != TrainingRequestStatus.Draft)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Only draft requests can be updated.");
        }

        var validationMessage = ValidateTrainingRequest(request);

        if (validationMessage != null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(validationMessage);
        }

        trainingRequest.Title = request.Title.Trim();
        trainingRequest.Provider = request.Provider.Trim();
        trainingRequest.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
        trainingRequest.City = request.City.Trim();
        trainingRequest.TrainingStartDate = request.TrainingStartDate;
        trainingRequest.TrainingEndDate = request.TrainingEndDate;
        trainingRequest.DepartureDate = request.DepartureDate;
        trainingRequest.ReturnDate = request.ReturnDate;
        trainingRequest.Justification = request.Justification.Trim();
        trainingRequest.CourseFee = Math.Round(
            request.CourseFee, 2, MidpointRounding.AwayFromZero);
        trainingRequest.CourseFeeCurrency = request.CourseFeeCurrency.Trim().ToUpperInvariant();
        trainingRequest.EstimatedFlightCost = Math.Round(
            request.EstimatedFlightCost, 2, MidpointRounding.AwayFromZero);
        trainingRequest.FlightCostCurrency = request.FlightCostCurrency.Trim().ToUpperInvariant();

        await _trainingRepository.UpdateAsync(trainingRequest);
        await _trainingRepository.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            null,
            "Training request draft updated successfully.");
    }

    public async Task<ApiResponse> SubmitForApprovalAsync(
        int employeeId,
        int requestId)
    {
        var trainingRequest =
            await _trainingRepository.GetByIdAsync(requestId);

        if (trainingRequest == null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Training request not found.");
        }

        if (trainingRequest.EmployeeId != employeeId)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("You are not authorized to submit this request.");
        }

        if (trainingRequest.Status != TrainingRequestStatus.Draft)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Only draft requests can be submitted.");
        }

        var employee =
            await _authRepository.GetEmployeeByIdAsync(employeeId);

        if (employee == null || !employee.IsActive)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Employee not found or inactive.");
        }

        if (employee.ReportingManagerId == null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("You must have an assigned reporting manager before submitting.");
        }

        var manager = await _authRepository.GetEmployeeByIdAsync(
            employee.ReportingManagerId.Value);

        if (manager == null || !manager.IsActive)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Your reporting manager is unavailable.");
        }

        trainingRequest.AssignedManagerId = manager.Id;
        trainingRequest.Status = TrainingRequestStatus.PendingManagerApproval;
        trainingRequest.SubmittedAtUtc = DateTime.UtcNow;

        await _trainingRepository.UpdateAsync(trainingRequest);
        await _trainingRepository.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            null,
            "Training request submitted for manager approval.");
    }

    public async Task<ApiResponse> ApproveByManagerAsync(int managerId, int requestId, ManagerTrainingDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId == managerId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You cannot approve your own training request.");
            }

            if (trainingRequest.AssignedManagerId != managerId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You are not authorized to approve this request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.PendingManagerApproval)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("This request is not awaiting manager approval.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("An approval comment is required.");
            }

            trainingRequest.ManagerComment = request.Comment.Trim();
            trainingRequest.ManagerDecisionAtUtc = DateTime.UtcNow;
            trainingRequest.Status = TrainingRequestStatus.PendingHRApproval;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request approved and forwarded to HR.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "An error occurred while approving the training request.");
        }
    }

    public async Task<ApiResponse> RejectByManagerAsync(int managerId, int requestId, ManagerTrainingDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId == managerId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You cannot reject your own training request.");
            }

            if (trainingRequest.AssignedManagerId != managerId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You are not authorized to reject this request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.PendingManagerApproval)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("This request is not awaiting manager approval.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("A rejection reason is required.");
            }

            trainingRequest.ManagerComment = request.Comment.Trim();
            trainingRequest.ManagerDecisionAtUtc = DateTime.UtcNow;
            trainingRequest.Status = TrainingRequestStatus.Rejected;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request rejected.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "An error occurred while rejecting the training request.");
        }
    }

    public async Task<ApiResponse> ApproveByHRAsync(int hrEmployeeId, int requestId, HRTrainingDecisionRequest request)
    {
         try
    {
        var trainingRequest =
            await _trainingRepository.GetByIdAsync(requestId);

        if (trainingRequest == null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("Training request not found.");
        }

        if (trainingRequest.EmployeeId == hrEmployeeId)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("You cannot approve your own training request.");
        }

        if (trainingRequest.Status != TrainingRequestStatus.PendingHRApproval)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("This request is not awaiting HR approval.");
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("An HR review comment is required.");
        }

        var employee = await _authRepository
            .GetEmployeeByIdAsync(trainingRequest.EmployeeId);

        if (employee == null || !employee.IsActive)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("The employee is not available.");
        }

        var rate = await _trainingRepository.GetEstacodeRateAsync(
            trainingRequest.CountryCode,
            employee.Grade);

        if (rate == null)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "No active Estacode rate exists for this country and employee grade.");
        }

        var eligibleDays = (
            trainingRequest.ReturnDate.Date -
            trainingRequest.DepartureDate.Date).Days + 1;

        if (eligibleDays <= 0)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("The travel dates are invalid.");
        }

        var totalAllowance = Math.Round(
            rate.DailyRateUsd * eligibleDays,
            2,
            MidpointRounding.AwayFromZero);

        var existingVersions = await _trainingRepository
            .GetAllowanceVersionsByRequestIdAsync(requestId);

        var nextVersion = existingVersions.Count + 1;

        var allowanceVersion = new TrainingAllowanceVersion
        {
            TrainingRequestId = requestId,
            VersionNumber = nextVersion,
            Grade = employee.Grade,
            CountryCode = trainingRequest.CountryCode,
            DailyRateUsd = rate.DailyRateUsd,
            Currency = "USD",
            EligibleDays = eligibleDays,
            TotalAllowance = totalAllowance,
            CorrectionReason = request.CorrectionReason,
            CalculatedByEmployeeId = hrEmployeeId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _trainingRepository.AddAllowanceVersionAsync(
            allowanceVersion);

        trainingRequest.HRDecisionAtUtc = DateTime.UtcNow;
        trainingRequest.Status = TrainingRequestStatus.PendingFinanceApproval;

        await _trainingRepository.UpdateAsync(trainingRequest);
        await _trainingRepository.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            new
            {
                TrainingRequestId = trainingRequest.Id,
                AllowanceVersion = nextVersion,
                Grade = employee.Grade,
                CountryCode = trainingRequest.CountryCode,
                DailyRateUsd = rate.DailyRateUsd,
                EligibleDays = eligibleDays,
                TotalAllowanceUsd = totalAllowance
            },
            "HR approval completed. Training request forwarded to Finance.");
    }
    catch (Exception)
    {
        return ApiResponse.ResponseHelper
            .FailureResponse(
                "An error occurred while processing HR approval.");
    }
    }

    public async Task<ApiResponse> ReturnToManagerAsync(int hrEmployeeId, int requestId, HRTrainingDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId == hrEmployeeId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You cannot return your own training request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.PendingHRApproval)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("This request is not awaiting HR approval.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("A reason for returning the request is required.");
            }

            trainingRequest.Status = TrainingRequestStatus.ReturnedToHR;
            trainingRequest.HRDecisionAtUtc = DateTime.UtcNow;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request returned for further review.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "An error occurred while returning the training request.");
        }
    }

    public async Task<ApiResponse> ApproveByFinanceAsync(int financeEmployeeId, int requestId, FinanceTrainingDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId == financeEmployeeId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You cannot approve your own training request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.PendingFinanceApproval)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("This request is not awaiting Finance approval.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("A Finance approval comment is required.");
            }

            trainingRequest.FinanceComment = request.Comment.Trim();
            trainingRequest.FinanceDecisionAtUtc = DateTime.UtcNow;
            trainingRequest.Status = TrainingRequestStatus.Approved;
            trainingRequest.ApprovedAtUtc = DateTime.UtcNow;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request approved by Finance.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "An error occurred while approving the training request.");
        }
    }

    public async Task<ApiResponse> RejectByFinanceAsync(int financeEmployeeId, int requestId, FinanceTrainingDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId == financeEmployeeId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You cannot reject your own training request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.PendingFinanceApproval)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("This request is not awaiting Finance approval.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("A Finance rejection reason is required.");
            }

            trainingRequest.FinanceComment = request.Comment.Trim();
            trainingRequest.FinanceDecisionAtUtc = DateTime.UtcNow;
            trainingRequest.Status = TrainingRequestStatus.Rejected;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request rejected by Finance.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse(
                    "An error occurred while rejecting the training request.");
        }
    }

    public async Task<ApiResponse> ReturnCompletionEvidenceAsync(int hrEmployeeId, int requestId, TrainingCompletionDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Training request not found.");
            }

            if (trainingRequest.EmployeeId == hrEmployeeId)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "You cannot review your own training request.");
            }

            if (trainingRequest.Status !=
                TrainingRequestStatus.CompletionEvidenceSubmitted)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Only submitted completion evidence can be returned.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A comment is required when returning completion evidence.");
            }

            trainingRequest.CompletionDecisionComment = request.Comment;
            trainingRequest.Status = TrainingRequestStatus.Approved;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Completion evidence returned to the employee for resubmission.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while returning completion evidence.");
        }
    }

    public async Task<ApiResponse> CompleteTrainingAsync(int hrEmployeeId, int requestId, TrainingCompletionDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Training request not found.");
            }

            if (trainingRequest.EmployeeId == hrEmployeeId)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "You cannot review your own training request.");
            }

            if (trainingRequest.Status !=
                TrainingRequestStatus.CompletionEvidenceSubmitted)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This request is not awaiting completion evidence review.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A decision comment is required.");
            }

            trainingRequest.CompletionDecisionComment = request.Comment;
            trainingRequest.CompletedAtUtc = DateTime.UtcNow;
            trainingRequest.Status = TrainingRequestStatus.Completed;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training completion confirmed successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while confirming training completion.");
        }
    }

    public async Task<ApiResponse> CloseWithoutEvidenceAsync(int hrEmployeeId, int requestId, TrainingCompletionDecisionRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Training request not found.");
            }

            if (trainingRequest.EmployeeId == hrEmployeeId)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "You cannot close your own training request.");
            }

            if (trainingRequest.Status != TrainingRequestStatus.Approved)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Only approved requests without submitted evidence can be closed this way.");
            }

            var daysSinceTrainingEnded =
                (DateTime.UtcNow.Date -
                 trainingRequest.TrainingEndDate.Date).Days;

            if (daysSinceTrainingEnded < 60)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "The request can only be closed without evidence 60 days after the training end date.");
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A reason is required to close a request without evidence.");
            }

            trainingRequest.CompletionDecisionComment = request.Comment;
            trainingRequest.Status = TrainingRequestStatus.RejectedNoEvidence;

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request closed because completion evidence was not submitted.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while closing the training request.");
        }
    }

    public async Task<ApiResponse> SubmitCompletionEvidenceAsync(int employeeId, int requestId, TrainingCompletionEvidenceRequest request)
    {
         try
         {
             var trainingRequest =
                 await _trainingRepository.GetByIdAsync(requestId);

             if (trainingRequest == null)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Training request not found.");
             }

             if (trainingRequest.EmployeeId != employeeId)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "You are not authorized to submit evidence for this request.");
             }

             if (trainingRequest.Status != TrainingRequestStatus.Approved)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Completion evidence can only be submitted for approved requests.");
             }

             if (DateTime.UtcNow.Date <= trainingRequest.TrainingEndDate.Date)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Evidence can only be uploaded after the training end date.");
             }

             if (request.EvidenceFile == null ||
                 request.EvidenceFile.Length == 0)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "A completion evidence file is required.");
             }

             const long maximumFileSize = 5 * 1024 * 1024;

             if (request.EvidenceFile.Length > maximumFileSize)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "The evidence file must not exceed 5 MB.");
             }

             if (string.IsNullOrWhiteSpace(request.Comment))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "An evidence comment is required.");
             }

             var extension = Path.GetExtension(
                 request.EvidenceFile.FileName).ToLowerInvariant();

             var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

             if (!allowedExtensions.Contains(extension))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Only PDF, JPG, and PNG files are allowed.");
             }

             var allowedContentTypes = new Dictionary<string, string[]>
             {
                 [".pdf"] = new[] { "application/pdf" },
                 [".jpg"] = new[] { "image/jpeg" },
                 [".jpeg"] = new[] { "image/jpeg" },
                 [".png"] = new[] { "image/png" }
             };

             if (!allowedContentTypes[extension].Contains(
                     request.EvidenceFile.ContentType,
                     StringComparer.OrdinalIgnoreCase))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "The uploaded file content type does not match the file extension.");
             }

             var evidenceDirectory = Path.Combine(
                 _environment.ContentRootPath,
                 "PrivateFiles",
                 "TrainingEvidence");

             Directory.CreateDirectory(evidenceDirectory);

             var storedFileName = $"{Guid.NewGuid():N}{extension}";
             var fullPath = Path.Combine(evidenceDirectory, storedFileName);

             await using (var stream = new FileStream(
                              fullPath,
                              FileMode.CreateNew,
                              FileAccess.Write,
                              FileShare.None))
             {
                 await request.EvidenceFile.CopyToAsync(stream);
             }

             try
             {
                 trainingRequest.CompletionEvidencePath =
                     Path.Combine(
                         "PrivateFiles",
                         "TrainingEvidence",
                         storedFileName);

                 trainingRequest.CompletionEvidenceFileName =
                     Path.GetFileName(request.EvidenceFile.FileName);

                 trainingRequest.CompletionEvidenceContentType =
                     request.EvidenceFile.ContentType;

                 trainingRequest.CompletionEvidenceComment = request.Comment;
                 trainingRequest.CompletionEvidenceSubmittedAtUtc = DateTime.UtcNow;
                 trainingRequest.CompletionEvidenceSubmittedByEmployeeId = employeeId;

                 trainingRequest.Status =
                     TrainingRequestStatus.CompletionEvidenceSubmitted;

                 await _trainingRepository.UpdateAsync(trainingRequest);
                 await _trainingRepository.SaveChangesAsync();
             }
             catch
             {
                 if (File.Exists(fullPath))
                 {
                     File.Delete(fullPath);
                 }

                 throw;
             }

             return ApiResponse.ResponseHelper.SuccessResponse(
                 null,
                 "Completion evidence uploaded successfully.");
         }
         catch (Exception)
         {
             return ApiResponse.ResponseHelper.FailureResponse(
                 "An error occurred while uploading completion evidence.");
         }
    }

    public async Task<ApiResponse> DownloadCompletionEvidenceAsync(int employeeId, int requestId)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Training request not found.");
            }

            if (trainingRequest.EmployeeId != employeeId &&
                trainingRequest.AssignedManagerId != employeeId)
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("You are not authorized to download this evidence.");
            }

            if (string.IsNullOrWhiteSpace(
                    trainingRequest.CompletionEvidencePath))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Completion evidence not found.");
            }

            var filePath = Path.Combine(
                _environment.ContentRootPath,
                trainingRequest.CompletionEvidencePath);

            if (!File.Exists(filePath))
            {
                return ApiResponse.ResponseHelper
                    .FailureResponse("Evidence file not found.");
            }

            var fileContent = await File.ReadAllBytesAsync(filePath);

            var downloadResponse = new TrainingEvidenceDownloadResponse
            {
                FileContent = fileContent,
                FileName = trainingRequest.CompletionEvidenceFileName
                           ?? "completion-evidence",
                ContentType = trainingRequest.CompletionEvidenceContentType
                              ?? "application/octet-stream"
            };

            return ApiResponse.ResponseHelper.SuccessResponse(
                downloadResponse,
                "Evidence retrieved successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper
                .FailureResponse("An error occurred while downloading the evidence.");
        }
    }

    public async Task<ApiResponse> WithdrawTrainingRequestAsync(int employeeId, int requestId, WithdrawTrainingRequest request)
    {
        try
        {
            var trainingRequest =
                await _trainingRepository.GetByIdAsync(requestId);

            if (trainingRequest == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Training request not found.");
            }

            if (trainingRequest.EmployeeId != employeeId)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "You are not authorized to withdraw this request.");
            }

            var withdrawableStatuses = new[]
            {
                TrainingRequestStatus.Draft,
                TrainingRequestStatus.PendingManagerApproval,
                TrainingRequestStatus.PendingHRApproval,
                TrainingRequestStatus.PendingFinanceApproval,
                TrainingRequestStatus.ReturnedToHR
            };

            if (!withdrawableStatuses.Contains(trainingRequest.Status))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This training request cannot be withdrawn in its current status.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "A withdrawal reason is required.");
            }

            trainingRequest.Status = TrainingRequestStatus.Withdrawn;

            trainingRequest.CompletionDecisionComment = request.Reason.Trim();

            await _trainingRepository.UpdateAsync(trainingRequest);
            await _trainingRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Training request withdrawn successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while withdrawing the training request.");
        }
    }

    public async Task<ApiResponse> ReassignTrainingReviewerAsync(int adminEmployeeId, int requestId, ReassignTrainingReviewerRequest request)
    {
         try
         {
             var trainingRequest =
                 await _trainingRepository.GetByIdAsync(requestId);

             if (trainingRequest == null)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Training request not found.");
             }

             if (trainingRequest.Status !=
                 TrainingRequestStatus.PendingManagerApproval)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "A reviewer can only be reassigned while the request is awaiting manager approval.");
             }

             if (string.IsNullOrWhiteSpace(request.Reason))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "A reason for reassignment is required.");
             }

             if (request.NewReviewerEmployeeId == trainingRequest.EmployeeId)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "The requester cannot be assigned as their own reviewer.");
             }

             var newReviewer =
                 await _authRepository.GetEmployeeByIdAsync(
                     request.NewReviewerEmployeeId);

             if (newReviewer == null || !newReviewer.IsActive)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "The new reviewer does not exist or is inactive.");
             }

             if (newReviewer.ReportingManagerId == null &&
                 newReviewer.Id == trainingRequest.EmployeeId)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "The requester cannot be assigned as their own reviewer.");
             }
             

             trainingRequest.AssignedManagerId = newReviewer.Id;

             trainingRequest.ManagerComment =
                 $"Reviewer reassigned by admin {adminEmployeeId}. Reason: {request.Reason.Trim()}";

             await _trainingRepository.UpdateAsync(trainingRequest);
             await _trainingRepository.SaveChangesAsync();
             
             await _emailService.SendEmailAsync(
                 newReviewer.Email,
                 "Training Request Assigned to You",
                 $"Hello {newReviewer.FirstName},\n\n" +
                 $"Training request #{trainingRequest.Id} has been assigned to you for manager review.\n" +
                 $"Training: {trainingRequest.Title}\n" +
                 $"Reason for reassignment: {request.Reason.Trim()}\n\n" +
                 "Please log in to review the request.");

             return ApiResponse.ResponseHelper.SuccessResponse(
                 new
                 {
                     TrainingRequestId = trainingRequest.Id,
                     NewReviewerEmployeeId = newReviewer.Id,
                     NewReviewerName = $"{newReviewer.FirstName} {newReviewer.LastName}"
                 },
                 "Training reviewer reassigned successfully.");
         }
         catch (Exception)
         {
             return ApiResponse.ResponseHelper.FailureResponse(
                 "An error occurred while reassigning the training reviewer.");
         }
    }

    public async Task<ApiResponse> GetPendingManagerRequestsAsync(int adminEmployeeId)
    {
        try
        {
            var admin = await _authRepository.GetEmployeeByIdAsync(adminEmployeeId);

            if (admin == null || !admin.IsActive)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Admin account not found or inactive.");
            }

            var requests = await _trainingRepository.GetPendingManagerRequestsAsync();

            var response = requests.Select(t => new TrainingRequestResponse
            {
                Id = t.Id,
                Title = t.Title,
                Provider = t.Provider,
                CountryCode = t.CountryCode,
                City = t.City,
                TrainingStartDate = t.TrainingStartDate,
                TrainingEndDate = t.TrainingEndDate,
                DepartureDate = t.DepartureDate,
                ReturnDate = t.ReturnDate,
                Justification = t.Justification,
                CourseFee = t.CourseFee,
                CourseFeeCurrency = t.CourseFeeCurrency,
                EstimatedFlightCost = t.EstimatedFlightCost,
                FlightCostCurrency = t.FlightCostCurrency,
                Status = t.Status.ToString(),
                EmployeeId = t.EmployeeId,
                AssignedManagerId = t.AssignedManagerId,
                CreatedAtUtc = t.CreatedAtUtc,
                SubmittedAtUtc = t.SubmittedAtUtc
            }).ToList();

            return ApiResponse.ResponseHelper.SuccessResponse(
                response,
                "Pending manager requests retrieved successfully.");
        }
        catch (Exception)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while retrieving pending manager requests.");
        }
    }

    private static string? ValidateTrainingRequest(
        CreateTrainingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Provider) ||
            string.IsNullOrWhiteSpace(request.CountryCode) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Justification) ||
            string.IsNullOrWhiteSpace(request.CourseFeeCurrency) ||
            string.IsNullOrWhiteSpace(request.FlightCostCurrency))
        {
            return "Please complete all required fields.";
        }

        if (request.TrainingEndDate.Date < request.TrainingStartDate.Date)
        {
            return "Training end date cannot be before training start date.";
        }

        if (request.DepartureDate.Date > request.TrainingStartDate.Date)
        {
            return "Departure date cannot be after training starts.";
        }

        if (request.ReturnDate.Date < request.TrainingEndDate.Date)
        {
            return "Return date cannot be before training ends.";
        }

        if (request.CourseFee < 0 || request.EstimatedFlightCost < 0)
        {
            return "Costs cannot be negative.";
        }

        return null;
    }

    private static string? ValidateTrainingRequest(
        UpdateTrainingRequest request)
    {
        return ValidateTrainingRequest(new CreateTrainingRequest
        {
            Title = request.Title,
            Provider = request.Provider,
            CountryCode = request.CountryCode,
            City = request.City,
            TrainingStartDate = request.TrainingStartDate,
            TrainingEndDate = request.TrainingEndDate,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            Justification = request.Justification,
            CourseFee = request.CourseFee,
            CourseFeeCurrency = request.CourseFeeCurrency,
            EstimatedFlightCost = request.EstimatedFlightCost,
            FlightCostCurrency = request.FlightCostCurrency
        });
    }
}