using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class QAEntryRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertAsync(ManualQAEntryEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT id FROM dbo.sp_insertmanualqaentry(@p_projectid, @p_modulename::varchar, @p_issuetype::varchar, @p_severity::varchar, @p_description::varchar, @p_reportedby::varchar)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_projectid",   entity.ProjectId);
        cmd.Parameters.AddWithValue("@p_modulename",  entity.ModuleName);
        cmd.Parameters.AddWithValue("@p_issuetype",   entity.IssueType);
        cmd.Parameters.AddWithValue("@p_severity",    entity.Severity);
        cmd.Parameters.AddWithValue("@p_description", (object?)entity.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_reportedby",  (object?)entity.ReportedBy  ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}

