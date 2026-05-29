using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
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

    public async Task<IEnumerable<AppointmentResponse>> GetUpcomingAsync()
    {
        var appointments = await _repo.GetUpcomingAsync();
        return appointments.Select(MapResponse);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetByDoctorAsync(int doctorId)
    {
        var appointments = await _repo.GetByDoctorAsync(doctorId);
        return appointments.Select(MapResponse);
    }

    private static AppointmentResponse MapResponse(Appointment appointment)
        => new()
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt
        };
}
