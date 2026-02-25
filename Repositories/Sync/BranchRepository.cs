using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class BranchRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertOrUpdateAsync(BranchEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertOrUpdateBranch", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@ProjectId",    entity.ProjectId);
        cmd.Parameters.AddWithValue("@BranchName",   entity.BranchName);
        cmd.Parameters.AddWithValue("@IsMain",       entity.IsMain);
        cmd.Parameters.AddWithValue("@AnalysisDate", (object?)entity.AnalysisDate ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
