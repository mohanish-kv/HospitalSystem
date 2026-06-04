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

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        const string sql = @"
            SELECT PatientId, PatientCode, FullName, DateOfBirth, Gender,
                   PhoneNumber, Email, IsActive, CreatedAt
            FROM Patients;";

        var patients = new List<Patient>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
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
        const string sql = @"
            SELECT PatientId, PatientCode, FullName, DateOfBirth, Gender,
                   PhoneNumber, Email, IsActive, CreatedAt
            FROM Patients
            WHERE PatientId = @PatientId;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.AddWithValue("@PatientId", id);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapPatient(reader) : null;
    }

    public async Task UpdateAsync(Patient patient)
    {
        const string sql = @"
            UPDATE Patients
            SET FullName = @FullName,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                IsActive = @IsActive
            WHERE PatientId = @PatientId;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.AddWithValue("@PatientId", patient.Id);
        cmd.Parameters.AddWithValue("@FullName", patient.FullName);
        cmd.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber);
        cmd.Parameters.AddWithValue("@Email", (object?)patient.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", patient.IsActive);

        await conn.OpenAsync();
        var affectedRows = await cmd.ExecuteNonQueryAsync();
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException($"Patient {patient.Id} not found.");
        }
    }

    public async Task DeactivateAsync(int id)
    {
        const string sql = @"
            UPDATE Patients
            SET IsActive = 0
            WHERE PatientId = @PatientId
              AND IsActive = 1;";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text
        };
        cmd.Parameters.AddWithValue("@PatientId", id);

        await conn.OpenAsync();
        var affectedRows = await cmd.ExecuteNonQueryAsync();
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException($"Patient {id} not found.");
        }
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
