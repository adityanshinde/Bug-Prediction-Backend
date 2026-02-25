using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class QualityGateConditionRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(QualityGateConditionEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertQualityGateCondition", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@SnapshotId",     entity.SnapshotId);
        cmd.Parameters.AddWithValue("@MetricKey",      entity.MetricKey);
        cmd.Parameters.AddWithValue("@Comparator",     (object?)entity.Comparator     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ErrorThreshold", (object?)entity.ErrorThreshold ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ActualValue",    (object?)entity.ActualValue    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status",         entity.Status);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}
