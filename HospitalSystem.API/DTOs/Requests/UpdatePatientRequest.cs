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

    public bool? IsActive { get; set; }

    [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be either 'Active' or 'Inactive'.")]
    public string? Status { get; set; }
}
