namespace HospitalSystem.API.Domain.Entities;

public class Doctor : Person
{
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsAvailable { get; set; }
}
