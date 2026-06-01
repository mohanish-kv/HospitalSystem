using System.Data;
using System.Data.Common;
using HospitalSystem.API.DTOs.Responses;
using HospitalSystem.API.Interfaces;
using Microsoft.Data.SqlClient;

namespace HospitalSystem.API.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("HospitalDb")!;

    public async Task<IEnumerable<ConsolidatedAppointmentReportResponse>> GetConsolidatedAsync()
    {
        const string sql = @"
            SELECT a.AppointmentId,
                   a.AppointmentDate,
                   a.Status,
                   p.PatientId,
                   p.FullName AS PatientName,
                   d.DoctorId,
                   d.FullName AS DoctorName,
                   d.Specialization,
                   d.ConsultationFee AS Fee
            FROM Appointments a
            JOIN Patients p ON a.PatientId = p.PatientId
            JOIN Doctors d ON a.DoctorId = d.DoctorId
            ORDER BY a.AppointmentDate DESC;";

        var rows = new List<ConsolidatedAppointmentReportResponse>();
        await ExecuteReaderAsync(["sp_ConsolidatedReport", "sp_GetConsolidatedAppointmentReport"], reader => rows.Add(new ConsolidatedAppointmentReportResponse
        {
            AppointmentId = GetInt32(reader, "AppointmentId"),
            AppointmentDate = GetDateTime(reader, "AppointmentDate"),
            Status = GetString(reader, "Status"),
            PatientId = GetInt32(reader, "PatientId"),
            PatientName = GetString(reader, "PatientName"),
            DoctorId = GetInt32(reader, "DoctorId"),
            DoctorName = GetString(reader, "DoctorName"),
            Specialization = GetString(reader, "Specialization"),
            Fee = GetDecimal(reader, "Fee")
        }));

        return rows;
    }

    public async Task<IEnumerable<DoctorAppointmentCountResponse>> GetDoctorCountsAsync()
    {
        const string sql = @"
            SELECT d.DoctorId,
                   d.FullName AS DoctorName,
                   d.Specialization,
                   COUNT(*) AS AppointmentCount
            FROM Appointments a
            JOIN Doctors d ON a.DoctorId = d.DoctorId
            GROUP BY d.DoctorId, d.FullName, d.Specialization
            HAVING COUNT(*) > 2
            ORDER BY AppointmentCount DESC;";

        var rows = new List<DoctorAppointmentCountResponse>();
        await ExecuteReaderAsync(["sp_DoctorAppointmentCount", "sp_GetDoctorsWithMoreThanTwoAppointments"], reader => rows.Add(new DoctorAppointmentCountResponse
        {
            DoctorId = GetInt32(reader, "DoctorId"),
            DoctorName = GetString(reader, "DoctorName"),
            Specialization = GetString(reader, "Specialization"),
            AppointmentCount = GetInt32(reader, "AppointmentCount")
        }));

        return rows;
    }

    public async Task<IEnumerable<RevenueBySpecializationResponse>> GetRevenueAsync()
    {
        const string sql = @"
            SELECT d.Specialization,
                   SUM(d.ConsultationFee) AS TotalRevenue
            FROM Appointments a
            JOIN Doctors d ON a.DoctorId = d.DoctorId
            WHERE a.Status = 'Completed'
            GROUP BY d.Specialization
            ORDER BY TotalRevenue DESC;";

        var rows = new List<RevenueBySpecializationResponse>();
        await ExecuteReaderAsync(["sp_RevenueBySpecialization", "sp_GetRevenueBySpecialization"], reader => rows.Add(new RevenueBySpecializationResponse
        {
            Specialization = GetString(reader, "Specialization"),
            TotalRevenue = GetDecimal(reader, "TotalRevenue")
        }));

        return rows;
    }

    public async Task<IEnumerable<DuplicateAppointmentResponse>> GetDuplicatesAsync()
    {
        const string sql = @"
            SELECT p.PatientId,
                   p.FullName AS PatientName,
                   d.DoctorId,
                   d.FullName AS DoctorName,
                   CAST(a.AppointmentDate AS DATE) AS AppointmentDay,
                   COUNT(*) AS AppointmentCount
            FROM Appointments a
            JOIN Patients p ON a.PatientId = p.PatientId
            JOIN Doctors d ON a.DoctorId = d.DoctorId
            WHERE a.Status <> 'Cancelled'
            GROUP BY p.PatientId, p.FullName, d.DoctorId, d.FullName, CAST(a.AppointmentDate AS DATE)
            HAVING COUNT(*) > 1
            ORDER BY AppointmentCount DESC;";

        var rows = new List<DuplicateAppointmentResponse>();
        await ExecuteReaderAsync(["sp_DuplicateDayBookings", "sp_GetDuplicatePatientDoctorAppointments"], reader => rows.Add(new DuplicateAppointmentResponse
        {
            PatientId = GetInt32(reader, "PatientId"),
            PatientName = GetString(reader, "PatientName"),
            DoctorId = GetInt32(reader, "DoctorId"),
            DoctorName = GetString(reader, "DoctorName", "DoctorFullName"),
            AppointmentDay = DateOnly.FromDateTime(GetDateTime(reader, "AppointmentDay", "AppointmentDate")),
            AppointmentCount = GetInt32(reader, "AppointmentCount", "DuplicateCount", "PatientCount")
        }));

        return rows;
    }

    private async Task ExecuteReaderAsync(string[] storedProcedures, Action<DbDataReader> readRow)
    {
        for (var i = 0; i < storedProcedures.Length; i++)
        {
            try
            {
                await ExecuteSingleReaderAsync(storedProcedures[i], readRow);
                return;
            }
            catch (SqlException ex) when (IsMissingStoredProcedure(ex) && i < storedProcedures.Length - 1)
            {
                // Try the next configured report stored procedure name for compatibility
                // with databases that use either the SQL script names or API names.
            }
        }
    }

    private async Task ExecuteSingleReaderAsync(string storedProcedure, Action<DbDataReader> readRow)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            readRow(reader);
        }
    }

    private static bool IsMissingStoredProcedure(SqlException ex)
        => ex.Errors.Cast<SqlError>().Any(error => error.Number == 2812);

    private static int GetInt32(DbDataReader reader, params string[] columnNames)
        => Convert.ToInt32(GetValue(reader, columnNames) ?? 0);

    private static decimal GetDecimal(DbDataReader reader, params string[] columnNames)
        => Convert.ToDecimal(GetValue(reader, columnNames) ?? 0m);

    private static DateTime GetDateTime(DbDataReader reader, params string[] columnNames)
        => Convert.ToDateTime(GetValue(reader, columnNames) ?? default(DateTime));

    private static string GetString(DbDataReader reader, params string[] columnNames)
        => Convert.ToString(GetValue(reader, columnNames)) ?? string.Empty;

    private static object? GetValue(DbDataReader reader, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (TryGetOrdinal(reader, columnName, out var ordinal))
            {
                return reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
            }
        }

        return null;
    }

    private static bool TryGetOrdinal(DbDataReader reader, string columnName, out int ordinal)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                ordinal = i;
                return true;
            }
        }

        ordinal = -1;
        return false;
    }
}
