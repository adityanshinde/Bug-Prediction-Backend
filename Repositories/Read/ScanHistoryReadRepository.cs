using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class ScanHistoryReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<ScanHistoryDto>> GetByProjectAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetSnapshotsByProject", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<ScanHistoryDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new ScanHistoryDto
            {
                ScanDate          = reader.GetDateTime(reader.GetOrdinal("ScanDate")),
                Branch            = reader.IsDBNull(reader.GetOrdinal("Branch"))            ? null : reader.GetString(reader.GetOrdinal("Branch")),
                CommitId          = reader.IsDBNull(reader.GetOrdinal("CommitId"))          ? null : reader.GetString(reader.GetOrdinal("CommitId")),
                QualityGateStatus = reader.IsDBNull(reader.GetOrdinal("QualityGateStatus")) ? null : reader.GetString(reader.GetOrdinal("QualityGateStatus")),
                Bugs              = reader.IsDBNull(reader.GetOrdinal("Bugs"))              ? 0 : reader.GetInt32(reader.GetOrdinal("Bugs")),
                Vulnerabilities   = reader.IsDBNull(reader.GetOrdinal("Vulnerabilities"))   ? 0 : reader.GetInt32(reader.GetOrdinal("Vulnerabilities")),
                CodeSmells        = reader.IsDBNull(reader.GetOrdinal("CodeSmells"))        ? 0 : reader.GetInt32(reader.GetOrdinal("CodeSmells")),
                Coverage          = reader.IsDBNull(reader.GetOrdinal("Coverage"))          ? 0 : reader.GetDecimal(reader.GetOrdinal("Coverage")),
                Duplication       = reader.IsDBNull(reader.GetOrdinal("Duplication"))       ? 0 : reader.GetDecimal(reader.GetOrdinal("Duplication"))
            });
        }
        return list;
    }
}
