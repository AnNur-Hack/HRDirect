namespace HRDirectAPI.DTOs.AuthRequests;

public class SetupAccountRequest
{
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}