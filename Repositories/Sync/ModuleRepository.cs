using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class ModuleRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(ModuleMetricEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT dbo.sp_insertmodulemetric(@p_snapshotid, @p_modulename::varchar, @p_qualifier::varchar, @p_path::varchar, @p_language::varchar, @p_bugs, @p_vulnerabilities, @p_codesmells, @p_coverage, @p_duplication, @p_complexity, @p_ncloc)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_snapshotid",      entity.SnapshotId);
        cmd.Parameters.AddWithValue("@p_modulename",      (object?)entity.ModuleName  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_qualifier",       (object?)entity.Qualifier   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_path",            (object?)entity.Path        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_language",        (object?)entity.Language    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_bugs",            entity.Bugs);
        cmd.Parameters.AddWithValue("@p_vulnerabilities", entity.Vulnerabilities);
        cmd.Parameters.AddWithValue("@p_codesmells",      entity.CodeSmells);
        cmd.Parameters.AddWithValue("@p_coverage",        entity.Coverage);
        cmd.Parameters.AddWithValue("@p_duplication",     entity.Duplication);
        cmd.Parameters.AddWithValue("@p_complexity",      entity.Complexity);
        cmd.Parameters.AddWithValue("@p_ncloc",           entity.Ncloc);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ModuleMetricEnt>> GetBySnapshotAsync(int snapshotId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getmodulesbysnapshot(@p_snapshotid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_snapshotid", snapshotId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<ModuleMetricEnt> modules = [];
        while (await reader.ReadAsync())
        {
            modules.Add(new ModuleMetricEnt
            {
                ModuleName      = r(reader, "ModuleName"),
                Qualifier       = r(reader, "Qualifier"),
                Path            = r(reader, "Path"),
                Language        = r(reader, "Language"),
                Bugs            = reader.IsDBNull(reader.GetOrdinal("Bugs"))            ? 0 : reader.GetInt32(reader.GetOrdinal("Bugs")),
                Vulnerabilities = reader.IsDBNull(reader.GetOrdinal("Vulnerabilities")) ? 0 : reader.GetInt32(reader.GetOrdinal("Vulnerabilities")),
                CodeSmells      = reader.IsDBNull(reader.GetOrdinal("CodeSmells"))      ? 0 : reader.GetInt32(reader.GetOrdinal("CodeSmells")),
                Coverage        = reader.IsDBNull(reader.GetOrdinal("Coverage"))        ? 0 : reader.GetDecimal(reader.GetOrdinal("Coverage")),
                Duplication     = reader.IsDBNull(reader.GetOrdinal("Duplication"))     ? 0 : reader.GetDecimal(reader.GetOrdinal("Duplication")),
                Complexity      = reader.IsDBNull(reader.GetOrdinal("Complexity"))      ? 0 : reader.GetInt32(reader.GetOrdinal("Complexity")),
                Ncloc           = reader.IsDBNull(reader.GetOrdinal("Ncloc"))           ? 0 : reader.GetInt32(reader.GetOrdinal("Ncloc"))
            });
        }
        return modules;
    }

    private static string? r(NpgsqlDataReader reader, string col)
        => reader.IsDBNull(reader.GetOrdinal(col)) ? null : reader.GetString(reader.GetOrdinal(col));
}

