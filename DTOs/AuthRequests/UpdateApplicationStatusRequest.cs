using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.DTOs.AuthRequests;

public class UpdateApplicationStatusRequest
{
    public OpportunityApplicationStatus Status { get; set; }

}