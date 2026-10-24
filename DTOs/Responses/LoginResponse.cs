namespace HRDirectAPI.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public EmployeeResponse Employee { get; set; }
}