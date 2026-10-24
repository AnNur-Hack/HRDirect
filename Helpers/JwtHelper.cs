using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.DTOs.Responses;
using Microsoft.IdentityModel.Tokens;

namespace HRDirectAPI.Helpers;

public class JwtHelper(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    public LoginResponse GenerateJwt(Employee employee)
    {
        var expiryMinutes = int.Parse(
            configuration["JWT:ExpirationInMinutes"]!);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var roles = employee.EmployeeRoleAssignments
            .Select(x => x.Role.Name)
            .ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
            new(ClaimTypes.Email, employee.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"]!,
            audience: configuration["JWT:Audience"]!,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            Employee = new EmployeeResponse
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Department = employee.Department,
                Grade = employee.Grade,
                Roles = roles
            }
        };
    }
}