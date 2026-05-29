using HospitalSystem.API.Domain.Entities;

namespace HospitalSystem.API.Interfaces;

public interface IAppointmentRepository
{
    Task<int> BookAsync(Appointment appointment);
    Task CancelAsync(int appointmentId);
    Task<IEnumerable<Appointment>> GetUpcomingAsync();
    Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId);
}
