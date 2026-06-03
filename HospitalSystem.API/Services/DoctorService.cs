using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class DoctorService
{
    private readonly IDoctorRepository _repo;

    public DoctorService(IDoctorRepository repo) => _repo = repo;

    public async Task<int> AddAsync(AddDoctorRequest req)
    {
        var doctor = new Doctor
        {
            Code = req.DoctorCode,
            FullName = req.FullName,
            Specialization = req.Specialization,
            PhoneNumber = req.PhoneNumber,
            ConsultationFee = req.ConsultationFee,
            IsAvailable = req.IsAvailable
        };

        return await _repo.AddAsync(doctor);
    }

    public async Task<IEnumerable<DoctorResponse>> GetAsync(string? specialization, bool? isAvailable)
    {
        var doctors = await _repo.GetAsync(specialization, isAvailable);

        return doctors.Select(MapDoctorResponse);
    }

    public async Task<DoctorResponse?> GetByIdAsync(int id)
    {
        var doctor = await _repo.GetByIdAsync(id);
        return doctor is null ? null : MapDoctorResponse(doctor);
    }

    private static DoctorResponse MapDoctorResponse(Doctor doctor)
        => new()
        {
            DoctorId = doctor.Id,
            DoctorCode = doctor.Code,
            FullName = doctor.FullName,
            Specialization = doctor.Specialization,
            PhoneNumber = doctor.PhoneNumber,
            Email = doctor.Email,
            ConsultationFee = doctor.ConsultationFee,
            IsAvailable = doctor.IsAvailable
        };
}
