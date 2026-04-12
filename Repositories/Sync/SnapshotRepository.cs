using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class SnapshotRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertAsync(SnapshotEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT id FROM dbo.sp_insertsnapshot(@p_projectid, @p_branchid, @p_scandate::timestamp, @p_commitid::varchar, @p_bugs, @p_vulnerabilities, @p_codesmells, @p_coverage, @p_duplication, @p_securityrating::varchar, @p_reliabilityrating::varchar, @p_maintainabilityrating::varchar, @p_qualitygatestatus::varchar)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_projectid",             entity.ProjectId);
        cmd.Parameters.AddWithValue("@p_branchid",              (object?)entity.BranchId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_scandate",              entity.ScanDate);
        cmd.Parameters.AddWithValue("@p_commitid",              (object?)entity.CommitId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_bugs",                  entity.Bugs);
        cmd.Parameters.AddWithValue("@p_vulnerabilities",       entity.Vulnerabilities);
        cmd.Parameters.AddWithValue("@p_codesmells",            entity.CodeSmells);
        cmd.Parameters.AddWithValue("@p_coverage",              entity.Coverage);
        cmd.Parameters.AddWithValue("@p_duplication",           entity.Duplication);
        cmd.Parameters.AddWithValue("@p_securityrating",        (object?)entity.SecurityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_reliabilityrating",     (object?)entity.ReliabilityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_maintainabilityrating", (object?)entity.MaintainabilityRating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_qualitygatestatus",     (object?)entity.QualityGateStatus ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<SnapshotEnt?> GetLatestAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getlatestsnapshot(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapSnapshot(reader) : null;
    }

    private static SnapshotEnt MapSnapshot(NpgsqlDataReader r) => new()
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

