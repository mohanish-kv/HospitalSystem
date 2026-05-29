using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class PatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo) => _repo = repo;

    public async Task<int> RegisterAsync(RegisterPatientRequest req)
    {
        var patient = new Patient
        {
            Code = req.PatientCode,
            FullName = req.FullName,
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender,
            PhoneNumber = req.PhoneNumber,
            Email = req.Email
        };

        return await _repo.RegisterAsync(patient);
    }

    public async Task<IEnumerable<PatientResponse>> GetAllActiveAsync()
    {
        var patients = await _repo.GetAllActiveAsync();

        return patients.Select(p => new PatientResponse
        {
            PatientId = p.Id,
            PatientCode = p.Code,
            FullName = p.FullName,
            Age = p.Age,
            Gender = p.Gender.ToString(),
            PhoneNumber = p.PhoneNumber,
            Email = p.Email
        });
    }

    public async Task<PatientResponse?> GetByIdAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        return patient is null
            ? null
            : new PatientResponse
            {
                PatientId = patient.Id,
                PatientCode = patient.Code,
                FullName = patient.FullName,
                Age = patient.Age,
                Gender = patient.Gender.ToString(),
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email
            };
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest req)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient {id} not found.");

        patient.FullName = req.FullName;
        patient.PhoneNumber = req.PhoneNumber;
        patient.Email = req.Email;

        await _repo.UpdateAsync(patient);
    }

    public async Task DeactivateAsync(int id)
        => await _repo.DeactivateAsync(id);
}
