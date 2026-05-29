namespace HospitalSystem.API.DTOs.Responses;

public class DoctorAppointmentCountResponse
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
}
