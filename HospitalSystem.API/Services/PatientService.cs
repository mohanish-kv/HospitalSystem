using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.Domain.Exceptions;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class PatientService
{
    private static readonly HashSet<char> AllowedGenders = new() { 'F', 'M', 'O' };
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
        => _patientRepository = patientRepository;

    public Task<IEnumerable<Patient>> GetActivePatientsAsync()
        => _patientRepository.GetAllActiveAsync();

    public Task<Patient?> GetPatientByIdAsync(int id)
    {
        EnsurePositiveId(id);
        return _patientRepository.GetByIdAsync(id);
    }

    public async Task<Patient> RegisterPatientAsync(Patient patient)
    {
        ValidatePatientForRegistration(patient);

        patient.Code = NormalizeRequired(patient.Code);
        patient.FullName = NormalizeRequired(patient.FullName);
        patient.Gender = char.ToUpperInvariant(patient.Gender);
        patient.PhoneNumber = NormalizeRequired(patient.PhoneNumber);
        patient.Email = NormalizeOptional(patient.Email);

        var id = await _patientRepository.RegisterAsync(patient);
        patient.Id = id;

        return await _patientRepository.GetByIdAsync(id) ?? patient;
    }

    public async Task<bool> UpdatePatientAsync(int id, Patient patient)
    {
        EnsurePositiveId(id);
        ValidatePatientForUpdate(patient);

        var existingPatient = await _patientRepository.GetByIdAsync(id);
        if (existingPatient is null)
        {
            return false;
        }

        patient.Id = id;
        patient.FullName = NormalizeRequired(patient.FullName);
        patient.PhoneNumber = NormalizeRequired(patient.PhoneNumber);
        patient.Email = NormalizeOptional(patient.Email);

        await _patientRepository.UpdateAsync(patient);
        return true;
    }

    public async Task<bool> DeactivatePatientAsync(int id)
    {
        EnsurePositiveId(id);

        var existingPatient = await _patientRepository.GetByIdAsync(id);
        if (existingPatient is null)
        {
            return false;
        }

        await _patientRepository.DeactivateAsync(id);
        return true;
    }

    private static void ValidatePatientForRegistration(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.Code))
        {
            throw new DomainException("Patient code is required.");
        }

        if (patient.DateOfBirth == default)
        {
            throw new DomainException("Patient date of birth is required.");
        }

        if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            throw new DomainException("Patient date of birth cannot be in the future.");
        }

        var normalizedGender = char.ToUpperInvariant(patient.Gender);
        if (!AllowedGenders.Contains(normalizedGender))
        {
            throw new DomainException("Patient gender must be F, M, or O.");
        }

        ValidatePatientForUpdate(patient);
    }

    private static void ValidatePatientForUpdate(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
        {
            throw new DomainException("Patient full name is required.");
        }

        if (string.IsNullOrWhiteSpace(patient.PhoneNumber))
        {
            throw new DomainException("Patient phone number is required.");
        }
    }

    private static void EnsurePositiveId(int id)
    {
        if (id <= 0)
        {
            throw new DomainException("Patient id must be greater than zero.");
        }
    }

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
