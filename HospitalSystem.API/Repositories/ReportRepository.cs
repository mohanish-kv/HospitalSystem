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
        var rows = new List<ConsolidatedAppointmentReportResponse>();
        await ExecuteReaderAsync(["sp_ConsolidatedReport", "sp_GetConsolidatedAppointmentReport"], reader => rows.Add(new ConsolidatedAppointmentReportResponse
        {
            AppointmentId = GetInt32(reader, "AppointmentId"),
            AppointmentDate = GetDateTime(reader, "AppointmentDate"),
            Status = GetString(reader, "Status"),
            PatientId = GetInt32(reader, "PatientId"),
            PatientName = GetString(reader, "PatientName", "PatientFullName", "FullName"),
            DoctorId = GetInt32(reader, "DoctorId"),
            DoctorName = GetString(reader, "DoctorName", "DoctorFullName"),
            Specialization = GetString(reader, "Specialization"),
            Fee = GetDecimal(reader, "Fee", "ConsultationFee", "Amount")
        }));

        return rows;
    }

    public async Task<IEnumerable<DoctorAppointmentCountResponse>> GetDoctorCountsAsync()
    {
        var rows = new List<DoctorAppointmentCountResponse>();
        await ExecuteReaderAsync(["sp_DoctorAppointmentCount", "sp_GetDoctorsWithMoreThanTwoAppointments"], reader => rows.Add(new DoctorAppointmentCountResponse
        {
            DoctorId = GetInt32(reader, "DoctorId"),
            DoctorName = GetString(reader, "DoctorName", "FullName"),
            Specialization = GetString(reader, "Specialization"),
            AppointmentCount = GetInt32(reader, "AppointmentCount", "TotalAppointments")
        }));

        return rows;
    }

    public async Task<IEnumerable<RevenueBySpecializationResponse>> GetRevenueAsync()
    {
        var rows = new List<RevenueBySpecializationResponse>();
        await ExecuteReaderAsync(["sp_RevenueBySpecialization", "sp_GetRevenueBySpecialization"], reader => rows.Add(new RevenueBySpecializationResponse
        {
            Specialization = GetString(reader, "Specialization"),
            TotalRevenue = GetDecimal(reader, "TotalRevenue", "Revenue")
        }));

        return rows;
    }

    public async Task<IEnumerable<DuplicateAppointmentResponse>> GetDuplicatesAsync()
    {
        var rows = new List<DuplicateAppointmentResponse>();
        await ExecuteReaderAsync(["sp_DuplicateDayBookings", "sp_GetDuplicatePatientDoctorAppointments"], reader => rows.Add(new DuplicateAppointmentResponse
        {
            PatientId = GetInt32(reader, "PatientId"),
            PatientName = GetString(reader, "PatientName", "PatientFullName", "FullName"),
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
        using var cmd = new SqlCommand(storedProcedure, conn)
        {
            CommandType = CommandType.StoredProcedure
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
