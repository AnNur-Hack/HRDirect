using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;

public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response.ResponseCode == "00")
        {
            return Ok(response);
        }

        return Unauthorized(response);
    }
    
    [HttpPost("setup-account")]
    public async Task<IActionResult> SetupAccount(SetupAccountRequest request)
    {
        var response = await _authService.SetupAccountAsync(request);

        if (response.ResponseCode == "00")
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var response = await _authService.ForgotPasswordAsync(request);

        if (response.ResponseCode == "00")
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request)
    {
        var response = await _authService.ResetPasswordAsync(request);

        if (response.ResponseCode == "00")
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
    
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var employeeIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(employeeIdClaim, out var employeeId))
        {
            return Unauthorized(
                ApiResponse.ResponseHelper.FailureResponse(
                    "Invalid authentication token."));
        }

        var response = await _authService.ChangePasswordAsync(
            employeeId,
            request);

        if (response.ResponseCode == "00")
        {
            return Ok(response);
        }

        return BadRequest(response);
    }
    
    
}