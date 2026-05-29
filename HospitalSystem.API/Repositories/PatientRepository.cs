using System.Data;
using HospitalSystem.API.Domain.Entities;
using HospitalSystem.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HospitalSystem.API.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly string _connectionString;

    public PatientRepository(IConfiguration config)
        => _connectionString = config.GetConnectionString("HospitalDb")!;

    public async Task<int> RegisterAsync(Patient patient)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_RegisterPatient", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@PatientCode", patient.Code);
        cmd.Parameters.AddWithValue("@FullName", patient.FullName);
        cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@Gender", patient.Gender.ToString());
        cmd.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber);
        cmd.Parameters.AddWithValue("@Email", (object?)patient.Email ?? DBNull.Value);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Patient>> GetAllActiveAsync()
    {
        var patients = new List<Patient>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_GetActivePatients", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            patients.Add(MapPatient(reader));
        }

        return patients;
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_GetPatientById", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@PatientId", id);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapPatient(reader) : null;
    }

    public async Task UpdateAsync(Patient patient)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_UpdatePatient", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@PatientId", patient.Id);
        cmd.Parameters.AddWithValue("@FullName", patient.FullName);
        cmd.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber);
        cmd.Parameters.AddWithValue("@Email", (object?)patient.Email ?? DBNull.Value);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_DeactivatePatient", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@PatientId", id);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    private static Patient MapPatient(SqlDataReader reader)
    {
        var gender = reader.GetRequiredString("Gender");

        return new Patient
        {
            Id = reader.GetRequiredInt32("PatientId"),
            Code = reader.GetRequiredString("PatientCode"),
            FullName = reader.GetRequiredString("FullName"),
            DateOfBirth = DateOnly.FromDateTime(reader.GetRequiredDateTime("DateOfBirth")),
            Gender = string.IsNullOrWhiteSpace(gender) ? '\0' : gender[0],
            PhoneNumber = reader.GetRequiredString("PhoneNumber"),
            Email = reader.GetNullableString("Email"),
            IsActive = reader.GetOptionalBoolean("IsActive") ?? true,
            CreatedAt = reader.GetOptionalDateTime("CreatedAt") ?? default
        };
    }
}
