using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class SeverityRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(SeverityDistributionEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT dbo.sp_insertseveritydistribution(@p_snapshotid, @p_blocker, @p_critical, @p_major, @p_minor, @p_info)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_snapshotid", entity.SnapshotId);
        cmd.Parameters.AddWithValue("@p_blocker",    entity.Blocker);
        cmd.Parameters.AddWithValue("@p_critical",   entity.Critical);
        cmd.Parameters.AddWithValue("@p_major",      entity.Major);
        cmd.Parameters.AddWithValue("@p_minor",      entity.Minor);
        cmd.Parameters.AddWithValue("@p_info",       entity.Info);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<SeverityDistributionEnt?> GetBySnapshotAsync(int snapshotId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getseveritybysnapshot(@p_snapshotid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_snapshotid", snapshotId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? new SeverityDistributionEnt
        {
            SnapshotId = snapshotId,
            Blocker    = reader.IsDBNull(reader.GetOrdinal("Blocker"))  ? 0 : reader.GetInt32(reader.GetOrdinal("Blocker")),
            Critical   = reader.IsDBNull(reader.GetOrdinal("Critical")) ? 0 : reader.GetInt32(reader.GetOrdinal("Critical")),
            Major      = reader.IsDBNull(reader.GetOrdinal("Major"))    ? 0 : reader.GetInt32(reader.GetOrdinal("Major")),
            Minor      = reader.IsDBNull(reader.GetOrdinal("Minor"))    ? 0 : reader.GetInt32(reader.GetOrdinal("Minor")),
            Info       = reader.IsDBNull(reader.GetOrdinal("Info"))     ? 0 : reader.GetInt32(reader.GetOrdinal("Info"))
        } : null;
    }
}

