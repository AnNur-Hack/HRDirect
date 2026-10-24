using HRDirectAPI.Domain.Entities;

namespace HRDirectAPI.Repositories.Interfaces;

public interface ITrainingRepository
{
    Task AddAsync(TrainingRequest trainingRequest);

    Task<TrainingRequest?> GetByIdAsync(int id);

    Task<List<TrainingRequest>> GetByEmployeeIdAsync(int employeeId);

    Task<List<TrainingRequest>> GetPendingManagerRequestsAsync();

    Task UpdateAsync(TrainingRequest trainingRequest);

    Task<EstaCodeRate?> GetEstacodeRateAsync(string countryCode, string grade);

    Task AddAllowanceVersionAsync(TrainingAllowanceVersion allowanceVersion);

    Task<List<TrainingAllowanceVersion>> GetAllowanceVersionsByRequestIdAsync(
        int trainingRequestId);

    Task SaveChangesAsync();
}