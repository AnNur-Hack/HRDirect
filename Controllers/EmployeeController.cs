using System.Security.Claims;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRDirectAPI.Controllers;

public class EmployeeController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;
    
    [HttpPost]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeRequest request)
    {
        var response = await _employeeService.CreateEmployeeAsync(request);

        if (response.ResponseCode != "00")
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var response = await _employeeService.GetAllEmployeesAsync();

        if (response.ResponseCode != "00")
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("{employeeNumber}")]
    public async Task<IActionResult> GetEmployeeByEmployeeNumber(
        string employeeNumber)
    {
        var response = await _employeeService
            .GetEmployeeByEmployeeNumberAsync(employeeNumber);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage == "Employee not found.")
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
    
    [HttpPut("{employeeNumber}")]
    public async Task<IActionResult> UpdateEmployee(
        string employeeNumber,
        [FromBody] UpdateEmployeeRequest request)
    {
        var response = await _employeeService.UpdateEmployeeAsync(
            employeeNumber,
            request);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage == "Employee not found.")
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPatch("{employeeNumber}/deactivate")]
    public async Task<IActionResult> DeactivateEmployee(
        string employeeNumber)
    {
        var employeeIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(employeeIdClaim, out var adminEmployeeId))
        {
            return Unauthorized(
                ApiResponse.ResponseHelper.FailureResponse(
                    "Invalid authentication token."));
        }

        var response = await _employeeService.DeactivateEmployeeAsync(
            employeeNumber,
            adminEmployeeId);

        if (response.ResponseCode != "00")
        {
            if (response.ResponseMessage == "Employee not found.")
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
}