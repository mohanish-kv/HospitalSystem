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
}
