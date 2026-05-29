using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.API.DTOs.Requests;

public class UpdatePatientRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }
}
