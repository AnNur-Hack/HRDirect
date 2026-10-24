using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Repositories.Implementations;

public class TrainingRepository(ApplicationDbContext context) : ITrainingRepository
{
    private readonly ApplicationDbContext _context = context;
    
    
    public async Task AddAsync(TrainingRequest trainingRequest)
    {
        await _context.TrainingRequests.AddAsync(trainingRequest);
    }

    public async Task<TrainingRequest?> GetByIdAsync(int id)
    {
        return await _context.TrainingRequests
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TrainingRequest>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.TrainingRequests
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<List<TrainingRequest>> GetPendingManagerRequestsAsync()
    {
        return await _context.TrainingRequests
            .Where(t => t.Status == TrainingRequestStatus.PendingManagerApproval)
            .OrderBy(t => t.SubmittedAtUtc)
            .ToListAsync();
    }

    public Task UpdateAsync(TrainingRequest trainingRequest)
    {
        _context.TrainingRequests.Update(trainingRequest);
        return Task.CompletedTask;
    }

    public async Task<EstaCodeRate?> GetEstacodeRateAsync(
        string countryCode,
        string grade)
    {
        return await _context.EstacodeRates
            .FirstOrDefaultAsync(x =>
                x.CountryCode == countryCode &&
                x.Grade == grade &&
                x.IsActive);
    }

    public async Task AddAllowanceVersionAsync(
        TrainingAllowanceVersion allowanceVersion)
    {
        await _context.TrainingAllowanceVersions.AddAsync(allowanceVersion);
    }

    public async Task<List<TrainingAllowanceVersion>> GetAllowanceVersionsByRequestIdAsync(
        int trainingRequestId)
    {
        return await _context.TrainingAllowanceVersions
            .Where(x => x.TrainingRequestId == trainingRequestId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}