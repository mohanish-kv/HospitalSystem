using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.API.DTOs.Requests;

public class BookAppointmentRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }
}
