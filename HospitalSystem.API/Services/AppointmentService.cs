using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _repo;

    public AppointmentService(IAppointmentRepository repo) => _repo = repo;

    public async Task<int> BookAsync(BookAppointmentRequest req)
    {
        var appointment = new Appointment
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            AppointmentDate = req.AppointmentDate
        };

        appointment.ValidateFutureDate();

        return await _repo.BookAsync(appointment);
    }

    public async Task CancelAsync(int appointmentId)
        => await _repo.CancelAsync(appointmentId);
}
