using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class DashboardReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<HeaderDto?> GetHeaderAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetProjectHeader", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

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
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetRecentScans", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);
        cmd.Parameters.AddWithValue("@TopN", topN);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

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
