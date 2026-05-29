using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class DoctorService
{
    private readonly IDoctorRepository _repo;

    public DoctorService(IDoctorRepository repo) => _repo = repo;

    public async Task<IEnumerable<DoctorResponse>> GetAsync(string? specialization, bool? isAvailable)
    {
        var doctors = await _repo.GetAsync(specialization, isAvailable);

        return doctors.Select(d => new DoctorResponse
        {
            DoctorId = d.Id,
            FullName = d.FullName,
            Specialization = d.Specialization,
            PhoneNumber = d.PhoneNumber,
            Email = d.Email,
            IsAvailable = d.IsAvailable
        });
    }
}
