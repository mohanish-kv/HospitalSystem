using System.ComponentModel.DataAnnotations;
using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly PatientService _patientService;

    public PatientsController(PatientService patientService)
        => _patientService = patientService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetActivePatients()
    {
        var patients = await _patientService.GetActivePatientsAsync();
        return Ok(patients.Select(PatientResponse.FromPatient));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> GetPatientById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        return patient is null ? NotFound() : Ok(PatientResponse.FromPatient(patient));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientResponse>> RegisterPatient(RegisterPatientRequest request)
    {
        var patient = await _patientService.RegisterPatientAsync(request.ToPatient());
        var response = PatientResponse.FromPatient(patient);

        return CreatedAtAction(nameof(GetPatientById), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(int id, UpdatePatientRequest request)
    {
        var updated = await _patientService.UpdatePatientAsync(id, request.ToPatient());
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePatient(int id)
    {
        var deactivated = await _patientService.DeactivatePatientAsync(id);
        return deactivated ? NoContent() : NotFound();
    }
}

public sealed record RegisterPatientRequest(
    [property: Required, StringLength(25)] string Code,
    [property: Required, StringLength(150)] string FullName,
    [property: Required] DateOnly DateOfBirth,
    [property: Required] char Gender,
    [property: Required, Phone, StringLength(30)] string PhoneNumber,
    [property: EmailAddress, StringLength(254)] string? Email)
{
    public Patient ToPatient()
        => new()
        {
            Code = Code,
            FullName = FullName,
            DateOfBirth = DateOfBirth,
            Gender = Gender,
            PhoneNumber = PhoneNumber,
            Email = Email
        };
}

public sealed record UpdatePatientRequest(
    [property: Required, StringLength(150)] string FullName,
    [property: Required, Phone, StringLength(30)] string PhoneNumber,
    [property: EmailAddress, StringLength(254)] string? Email)
{
    public Patient ToPatient()
        => new()
        {
            FullName = FullName,
            PhoneNumber = PhoneNumber,
            Email = Email
        };
}

public sealed record PatientResponse(
    int Id,
    string Code,
    string FullName,
    DateOnly DateOfBirth,
    char Gender,
    string PhoneNumber,
    string? Email,
    bool IsActive,
    DateTime CreatedAt)
{
    public static PatientResponse FromPatient(Patient patient)
        => new(
            patient.Id,
            patient.Code,
            patient.FullName,
            patient.DateOfBirth,
            patient.Gender,
            patient.PhoneNumber,
            patient.Email,
            patient.IsActive,
            patient.CreatedAt);
}
