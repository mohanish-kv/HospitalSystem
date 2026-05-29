using HospitalSystem.API.Domain.Entities;

namespace HospitalSystem.API.Interfaces;

public interface IPatientRepository
{
    Task<int> RegisterAsync(Patient patient);
    Task<IEnumerable<Patient>> GetAllActiveAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task UpdateAsync(Patient patient);
    Task DeactivateAsync(int id);
}
