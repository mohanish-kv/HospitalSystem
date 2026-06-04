using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class PatientService
{
    private readonly IPatientRepository _repo;
    private readonly IEmailService _emailService;

    public PatientService(IPatientRepository repo, IEmailService emailService)
    {
        _repo = repo;
        _emailService = emailService;
    }

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

        var patientId = await _repo.RegisterAsync(patient);

        await _emailService.SendEmailAsync(
            patient.Email,
            "Hospital registration successful",
            $"Dear {patient.FullName},\n\n" +
            "Your patient registration has been completed successfully.\n\n" +
            $"Patient Code: {patient.Code}\n" +
            $"Patient Id: {patientId}\n\n" +
            "Thank you,\nHospital System");

        return patientId;
    }

    public async Task<IEnumerable<PatientResponse>> GetAllActiveAsync()
    {
        var patients = await _repo.GetAllActiveAsync();

        return patients.Select(MapPatientResponse);
    }

    public async Task<PatientResponse?> GetByIdAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        return patient is null ? null : MapPatientResponse(patient);
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest req)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient {id} not found.");

        patient.FullName = req.FullName;
        patient.PhoneNumber = req.PhoneNumber;
        patient.Email = req.Email;
        patient.IsActive = ResolvePatientIsActive(req, patient.IsActive);

        await _repo.UpdateAsync(patient);
    }

    public async Task DeactivateAsync(int id)
        => await _repo.DeactivateAsync(id);

    private static bool ResolvePatientIsActive(UpdatePatientRequest request, bool currentIsActive)
    {
        var statusIsActive = request.Status?.Trim().ToUpperInvariant() switch
        {
            null or "" => (bool?)null,
            "ACTIVE" => true,
            "INACTIVE" => false,
            _ => throw new ArgumentException("Status must be either 'Active' or 'Inactive'.")
        };

        if (request.IsActive.HasValue && statusIsActive.HasValue && request.IsActive.Value != statusIsActive.Value)
        {
            throw new ArgumentException("IsActive and Status must represent the same patient status.");
        }

        return request.IsActive ?? statusIsActive ?? currentIsActive;
    }

    private static PatientResponse MapPatientResponse(Patient patient)
        => new()
        {
            PatientId = patient.Id,
            PatientCode = patient.Code,
            FullName = patient.FullName,
            Age = patient.Age,
            Gender = patient.Gender.ToString(),
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            IsActive = patient.IsActive,
            Status = patient.IsActive ? "Active" : "Inactive"
        };
}
