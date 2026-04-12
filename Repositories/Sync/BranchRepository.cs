using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class BranchRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertOrUpdateAsync(BranchEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT id FROM dbo.sp_insertorupdatebranch(@p_projectid, @p_branchname::varchar, @p_ismain, @p_analysisdate::timestamp)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_projectid",    entity.ProjectId);
        cmd.Parameters.AddWithValue("@p_branchname",   entity.BranchName);
        cmd.Parameters.AddWithValue("@p_ismain",       entity.IsMain);
        cmd.Parameters.AddWithValue("@p_analysisdate", (object?)entity.AnalysisDate ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}

