using System.Data;
using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HospitalSystem.API.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly string _connectionString;

    public AppointmentRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("HospitalDb")!;

    public async Task<int> BookAsync(Appointment appointment)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_BookAppointment", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
        cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
        cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task CancelAsync(int appointmentId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_CancelAppointment", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAsync()
    {
        const string sql = @"
            SELECT AppointmentId, PatientId, DoctorId, AppointmentDate, Status, CreatedAt
            FROM Appointments
            WHERE AppointmentDate BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
              AND Status = 'Scheduled'
            ORDER BY AppointmentDate;";

        var appointments = new List<Appointment>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            appointments.Add(MapAppointment(reader));
        }

        return appointments;
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId)
    {
        const string sql = @"
            SELECT AppointmentId, PatientId, DoctorId, AppointmentDate, Status, CreatedAt
            FROM Appointments
            WHERE DoctorId = @DoctorId
            ORDER BY AppointmentDate DESC;";

        var appointments = new List<Appointment>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.AddWithValue("@DoctorId", doctorId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            appointments.Add(MapAppointment(reader));
        }

        return appointments;
    }

    private static Appointment MapAppointment(SqlDataReader reader)
        => new()
        {
            Id = reader.GetRequiredInt32("AppointmentId"),
            PatientId = reader.GetRequiredInt32("PatientId"),
            DoctorId = reader.GetRequiredInt32("DoctorId"),
            AppointmentDate = reader.GetRequiredDateTime("AppointmentDate"),
            Status = reader.GetOptionalString("Status") ?? string.Empty,
            CreatedAt = reader.GetOptionalDateTime("CreatedAt") ?? default
        };
}
