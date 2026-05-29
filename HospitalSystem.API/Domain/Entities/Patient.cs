namespace HospitalSystem.API.Domain.Entities;

public class Patient : Person
{
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public char Gender { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
