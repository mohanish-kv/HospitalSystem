using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.API.DTOs.Requests;

public class RegisterPatientRequest
{
    [Required]
    public string PatientCode { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [RegularExpression("^[MFO]$")]
    public char Gender { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }
}
