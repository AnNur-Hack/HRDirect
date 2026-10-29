namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateKpiItemRequest
{
    public string Title { get; set; }

    public string Description { get; set; } 

    public decimal Target { get; set; }

    public string Unit { get; set; } 

    public decimal Weight { get; set; }
}