using System.Data;
using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HospitalSystem.API.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly string _connectionString;

    public DoctorRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("HospitalDb")!;

    public async Task<IEnumerable<Doctor>> GetAsync(string? specialization, bool? isAvailable)
    {
        var doctors = new List<Doctor>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_GetDoctors", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Specialization", (object?)specialization ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsAvailable", (object?)isAvailable ?? DBNull.Value);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            doctors.Add(MapDoctor(reader));
        }

        return doctors;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_GetDoctorById", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@DoctorId", id);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapDoctor(reader) : null;
    }

    private static Doctor MapDoctor(SqlDataReader reader)
        => new()
        {
            Id = reader.GetRequiredInt32("DoctorId"),
            FullName = reader.GetRequiredString("FullName"),
            Specialization = reader.GetRequiredString("Specialization"),
            PhoneNumber = reader.GetOptionalString("PhoneNumber") ?? string.Empty,
            Email = reader.GetOptionalString("Email"),
            IsAvailable = reader.GetOptionalBoolean("IsAvailable") ?? false
        };
}
