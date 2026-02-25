using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class QualityGateReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<QualityGateHistoryDto>> GetHistoryAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetQualityGateHistory", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<QualityGateHistoryDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new QualityGateHistoryDto
            {
                Date     = reader.GetDateTime(reader.GetOrdinal("Date")),
                Branch   = reader.IsDBNull(reader.GetOrdinal("Branch"))    ? null : reader.GetString(reader.GetOrdinal("Branch")),
                Status   = reader.IsDBNull(reader.GetOrdinal("Status"))    ? null : reader.GetString(reader.GetOrdinal("Status")),
                CommitId = reader.IsDBNull(reader.GetOrdinal("CommitId"))  ? null : reader.GetString(reader.GetOrdinal("CommitId"))
            });
        }
        return list;
    }
}
