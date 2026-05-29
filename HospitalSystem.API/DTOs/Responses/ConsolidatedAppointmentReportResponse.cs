namespace HospitalSystem.API.DTOs.Responses;

public class ConsolidatedAppointmentReportResponse
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}
