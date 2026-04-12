using System.Data;
using Npgsql;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class QualityGateReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<QualityGateHistoryDto>> GetHistoryAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getqualitygatehistory(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

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

