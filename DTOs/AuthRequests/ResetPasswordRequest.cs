namespace HRDirectAPI.DTOs.AuthRequests;

public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}