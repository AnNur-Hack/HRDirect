namespace HRDirectAPI.Domain.Enums;

public enum TrainingRequestStatus
{
    Draft = 1,
    PendingManagerApproval = 2,
    PendingHRApproval = 3,
    PendingFinanceApproval = 4,
    Approved = 5,
    Rejected = 6,
    ReturnedToHR = 7,
    Withdrawn = 8,
    CompletionEvidenceSubmitted = 9,
    Completed = 10,
    RejectedNoEvidence = 11
}