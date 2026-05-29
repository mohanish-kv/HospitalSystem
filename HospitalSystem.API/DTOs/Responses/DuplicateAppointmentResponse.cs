namespace HospitalSystem.API.DTOs.Responses;

public class DuplicateAppointmentResponse
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly AppointmentDay { get; set; }
    public int AppointmentCount { get; set; }
}
