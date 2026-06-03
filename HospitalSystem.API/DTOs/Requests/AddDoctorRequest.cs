using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.API.DTOs.Requests;

public class AddDoctorRequest
{
    [Required]
    public string DoctorCode { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; } = true;
}
