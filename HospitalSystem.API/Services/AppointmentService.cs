using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.DTOs.Requests;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;

namespace HospitalSystem.API.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _repo;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IEmailService _emailService;

    public AppointmentService(
        IAppointmentRepository repo,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        IEmailService emailService)
    {
        _repo = repo;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _emailService = emailService;
    }

    public async Task<int> BookAsync(BookAppointmentRequest req)
    {
        var patient = await _patientRepository.GetByIdAsync(req.PatientId)
            ?? throw new KeyNotFoundException($"Patient {req.PatientId} not found.");
        var doctor = await _doctorRepository.GetByIdAsync(req.DoctorId)
            ?? throw new KeyNotFoundException($"Doctor {req.DoctorId} not found.");

        var appointment = new Appointment
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            AppointmentDate = req.AppointmentDate
        };

        appointment.ValidateFutureDate();

        var appointmentId = await _repo.BookAsync(appointment);

        await _emailService.SendEmailAsync(
            patient.Email,
            "Appointment booked successfully",
            $"Dear {patient.FullName},\n\n" +
            "Your appointment has been booked successfully.\n\n" +
            $"Appointment Id: {appointmentId}\n" +
            $"Doctor: {doctor.FullName}\n" +
            $"Specialization: {doctor.Specialization}\n" +
            $"Appointment Date: {FormatAppointmentDate(appointment.AppointmentDate)}\n\n" +
            "Thank you,\nHospital System");

        return appointmentId;
    }

    public async Task CancelAsync(int appointmentId)
    {
        var appointment = await _repo.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException($"Appointment {appointmentId} not found.");
        var patient = await _patientRepository.GetByIdAsync(appointment.PatientId)
            ?? throw new KeyNotFoundException($"Patient {appointment.PatientId} not found.");
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId)
            ?? throw new KeyNotFoundException($"Doctor {appointment.DoctorId} not found.");

        await _repo.CancelAsync(appointmentId);

        await _emailService.SendEmailAsync(
            patient.Email,
            "Appointment cancelled",
            $"Dear {patient.FullName},\n\n" +
            "Your appointment has been cancelled.\n\n" +
            $"Appointment Id: {appointmentId}\n" +
            $"Doctor: {doctor.FullName}\n" +
            $"Appointment Date: {FormatAppointmentDate(appointment.AppointmentDate)}\n\n" +
            "Thank you,\nHospital System");
    }

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

    private static string FormatAppointmentDate(DateTime appointmentDate)
        => appointmentDate.ToString("yyyy-MM-dd HH:mm");

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
