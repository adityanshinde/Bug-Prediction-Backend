using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class ModuleRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(ModuleMetricEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertModuleMetric", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@SnapshotId",      entity.SnapshotId);
        cmd.Parameters.AddWithValue("@ModuleName",      (object?)entity.ModuleName  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Qualifier",       (object?)entity.Qualifier   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Path",            (object?)entity.Path        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Language",        (object?)entity.Language    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Bugs",            entity.Bugs);
        cmd.Parameters.AddWithValue("@Vulnerabilities", entity.Vulnerabilities);
        cmd.Parameters.AddWithValue("@CodeSmells",      entity.CodeSmells);
        cmd.Parameters.AddWithValue("@Coverage",        entity.Coverage);
        cmd.Parameters.AddWithValue("@Duplication",     entity.Duplication);
        cmd.Parameters.AddWithValue("@Complexity",      entity.Complexity);
        cmd.Parameters.AddWithValue("@Ncloc",           entity.Ncloc);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ModuleMetricEnt>> GetBySnapshotAsync(int snapshotId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetModulesBySnapshot", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@SnapshotId", snapshotId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

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

    private static string? r(SqlDataReader reader, string col)
        => reader.IsDBNull(reader.GetOrdinal(col)) ? null : reader.GetString(reader.GetOrdinal(col));
}
