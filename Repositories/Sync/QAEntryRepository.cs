using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class QAEntryRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertAsync(ManualQAEntryEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertManualQAEntry", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@ProjectId",   entity.ProjectId);
        cmd.Parameters.AddWithValue("@ModuleName",  entity.ModuleName);
        cmd.Parameters.AddWithValue("@IssueType",   entity.IssueType);
        cmd.Parameters.AddWithValue("@Severity",    entity.Severity);
        cmd.Parameters.AddWithValue("@Description", (object?)entity.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReportedBy",  (object?)entity.ReportedBy  ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
