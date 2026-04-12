using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class QualityGateConditionRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task InsertAsync(QualityGateConditionEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT dbo.sp_insertqualitygatecondition(@p_snapshotid, @p_metrickey::varchar, @p_comparator::varchar, @p_errorthreshold::varchar, @p_actualvalue::varchar, @p_status::varchar)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_snapshotid",     entity.SnapshotId);
        cmd.Parameters.AddWithValue("@p_metrickey",      entity.MetricKey);
        cmd.Parameters.AddWithValue("@p_comparator",     (object?)entity.Comparator     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_errorthreshold", (object?)entity.ErrorThreshold ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_actualvalue",    (object?)entity.ActualValue    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_status",         entity.Status);

        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}

