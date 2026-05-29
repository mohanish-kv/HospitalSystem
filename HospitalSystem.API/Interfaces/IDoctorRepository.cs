using HospitalSystem.API.Domain.Entities;

namespace HospitalSystem.API.Interfaces;

public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAsync(string? specialization, bool? isAvailable);
    Task<Doctor?> GetByIdAsync(int id);
}
