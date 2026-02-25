using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class SeverityRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(SeverityDistributionEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertSeverityDistribution", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@SnapshotId", entity.SnapshotId);
        cmd.Parameters.AddWithValue("@Blocker",    entity.Blocker);
        cmd.Parameters.AddWithValue("@Critical",   entity.Critical);
        cmd.Parameters.AddWithValue("@Major",      entity.Major);
        cmd.Parameters.AddWithValue("@Minor",      entity.Minor);
        cmd.Parameters.AddWithValue("@Info",       entity.Info);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<SeverityDistributionEnt?> GetBySnapshotAsync(int snapshotId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetSeverityBySnapshot", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@SnapshotId", snapshotId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

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
