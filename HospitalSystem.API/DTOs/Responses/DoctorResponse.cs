namespace HospitalSystem.API.DTOs.Responses;

public class DoctorResponse
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsAvailable { get; set; }
}
