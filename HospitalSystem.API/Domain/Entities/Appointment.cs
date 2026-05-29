using HospitalSystem.API.Domain.Exceptions;

namespace HospitalSystem.API.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public DateTime ScheduledAt
    {
        get => AppointmentDate;
        set => AppointmentDate = value;
    }

    public void ValidateFutureDate()
    {
        if (AppointmentDate <= DateTime.UtcNow)
        {
            throw new DomainException("Appointment date must be in the future.");
        }
    }
}
