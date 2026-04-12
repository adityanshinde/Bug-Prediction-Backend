using System.Data;
using Npgsql;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class DashboardReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<HeaderDto?> GetHeaderAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getprojectheader(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? new HeaderDto
        {
            ProjectName       = reader.GetString(reader.GetOrdinal("ProjectName")),
            Branch            = reader.IsDBNull(reader.GetOrdinal("Branch"))            ? string.Empty : reader.GetString(reader.GetOrdinal("Branch")),
            LastScanDate      = reader.IsDBNull(reader.GetOrdinal("LastScanDate"))      ? null : reader.GetDateTime(reader.GetOrdinal("LastScanDate")),
            QualityGateStatus = reader.IsDBNull(reader.GetOrdinal("QualityGateStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("QualityGateStatus")),
            CommitId          = reader.IsDBNull(reader.GetOrdinal("CommitId"))          ? null : reader.GetString(reader.GetOrdinal("CommitId"))
        } : null;
    }

    public async Task<List<RecentScanDto>> GetRecentScansAsync(int projectId, int topN = 10)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getrecentscans(@p_projectid, @p_topn)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);
        cmd.Parameters.AddWithValue("@p_topn", topN);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<RecentScanDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new RecentScanDto
            {
                ScanDate    = reader.GetDateTime(reader.GetOrdinal("ScanDate")),
                Branch      = reader.IsDBNull(reader.GetOrdinal("Branch"))      ? null : reader.GetString(reader.GetOrdinal("Branch")),
                Commit      = reader.IsDBNull(reader.GetOrdinal("CommitId"))    ? null : reader.GetString(reader.GetOrdinal("CommitId")),
                QualityGate = reader.IsDBNull(reader.GetOrdinal("QualityGateStatus")) ? null : reader.GetString(reader.GetOrdinal("QualityGateStatus"))
            });
        }
        return list;
    }
}

