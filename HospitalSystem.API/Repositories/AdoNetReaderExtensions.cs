using System.Data.Common;

namespace HospitalSystem.API.Repositories;

internal static class AdoNetReaderExtensions
{
    public static int GetRequiredInt32(this DbDataReader reader, string columnName)
        => reader.GetInt32(reader.GetOrdinal(columnName));

    public static string GetRequiredString(this DbDataReader reader, string columnName)
        => reader.GetString(reader.GetOrdinal(columnName));

    public static DateTime GetRequiredDateTime(this DbDataReader reader, string columnName)
        => reader.GetDateTime(reader.GetOrdinal(columnName));

    public static bool GetRequiredBoolean(this DbDataReader reader, string columnName)
        => reader.GetBoolean(reader.GetOrdinal(columnName));

    public static string? GetNullableString(this DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static string? GetOptionalString(this DbDataReader reader, string columnName)
        => reader.TryGetOrdinal(columnName, out var ordinal) && !reader.IsDBNull(ordinal)
            ? reader.GetString(ordinal)
            : null;

    public static DateTime? GetOptionalDateTime(this DbDataReader reader, string columnName)
        => reader.TryGetOrdinal(columnName, out var ordinal) && !reader.IsDBNull(ordinal)
            ? reader.GetDateTime(ordinal)
            : null;

    public static bool? GetOptionalBoolean(this DbDataReader reader, string columnName)
        => reader.TryGetOrdinal(columnName, out var ordinal) && !reader.IsDBNull(ordinal)
            ? reader.GetBoolean(ordinal)
            : null;

    private static bool TryGetOrdinal(this DbDataReader reader, string columnName, out int ordinal)
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
