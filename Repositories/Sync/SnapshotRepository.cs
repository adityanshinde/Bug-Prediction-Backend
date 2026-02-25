using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class SnapshotRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertAsync(SnapshotEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertSnapshot", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@ProjectId",             entity.ProjectId);
        cmd.Parameters.AddWithValue("@BranchId",              (object?)entity.BranchId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ScanDate",              entity.ScanDate);
        cmd.Parameters.AddWithValue("@CommitId",              (object?)entity.CommitId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Bugs",                  entity.Bugs);
        cmd.Parameters.AddWithValue("@Vulnerabilities",       entity.Vulnerabilities);
        cmd.Parameters.AddWithValue("@CodeSmells",            entity.CodeSmells);
        cmd.Parameters.AddWithValue("@Coverage",              entity.Coverage);
        cmd.Parameters.AddWithValue("@Duplication",           entity.Duplication);
        cmd.Parameters.AddWithValue("@SecurityRating",        (object?)entity.SecurityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReliabilityRating",     (object?)entity.ReliabilityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaintainabilityRating", (object?)entity.MaintainabilityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@QualityGateStatus",     (object?)entity.QualityGateStatus ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<SnapshotEnt?> GetLatestAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetLatestSnapshot", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapSnapshot(reader) : null;
    }

    private static SnapshotEnt MapSnapshot(SqlDataReader r) => new()
    {
        Id                    = r.GetInt32(r.GetOrdinal("Id")),
        ProjectId             = r.GetInt32(r.GetOrdinal("ProjectId")),
        BranchId              = r.IsDBNull(r.GetOrdinal("BranchId")) ? null : r.GetInt32(r.GetOrdinal("BranchId")),
        ScanDate              = r.GetDateTime(r.GetOrdinal("ScanDate")),
        CommitId              = r.IsDBNull(r.GetOrdinal("CommitId"))  ? null : r.GetString(r.GetOrdinal("CommitId")),
        Bugs                  = r.GetInt32(r.GetOrdinal("Bugs")),
        Vulnerabilities       = r.GetInt32(r.GetOrdinal("Vulnerabilities")),
        CodeSmells            = r.GetInt32(r.GetOrdinal("CodeSmells")),
        Coverage              = r.GetDecimal(r.GetOrdinal("Coverage")),
        Duplication           = r.GetDecimal(r.GetOrdinal("Duplication")),
        SecurityRating        = r.IsDBNull(r.GetOrdinal("SecurityRating"))        ? null : r.GetString(r.GetOrdinal("SecurityRating")),
        ReliabilityRating     = r.IsDBNull(r.GetOrdinal("ReliabilityRating"))     ? null : r.GetString(r.GetOrdinal("ReliabilityRating")),
        MaintainabilityRating = r.IsDBNull(r.GetOrdinal("MaintainabilityRating")) ? null : r.GetString(r.GetOrdinal("MaintainabilityRating")),
        QualityGateStatus     = r.IsDBNull(r.GetOrdinal("QualityGateStatus"))     ? null : r.GetString(r.GetOrdinal("QualityGateStatus"))
    };
}
