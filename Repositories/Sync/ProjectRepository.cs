using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class ProjectRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertOrUpdateAsync(ProjectEnt entity)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT id FROM dbo.sp_insertorupdateproject(@p_projectkey::varchar, @p_name::varchar, @p_organization::varchar, @p_visibility::varchar, @p_lastanalysisdate::timestamp)", con);
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.AddWithValue("@p_projectkey", entity.ProjectKey);
        cmd.Parameters.AddWithValue("@p_name", entity.Name);
        cmd.Parameters.AddWithValue("@p_organization", (object?)entity.Organization ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_visibility", (object?)entity.Visibility ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@p_lastanalysisdate", (object?)entity.LastAnalysisDate ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<ProjectEnt>> GetAllAsync()
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getallprojects()", con);
        cmd.CommandType = CommandType.Text;

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<ProjectEnt> projects = [];
        while (await reader.ReadAsync())
        {
            projects.Add(MapProject(reader));
        }
        return projects;
    }

    public async Task<ProjectEnt?> GetByIdAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getprojectbyid(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapProject(reader) : null;
    }

    private static ProjectEnt MapProject(NpgsqlDataReader r) => new()
    {
        Id              = r.GetInt32(r.GetOrdinal("Id")),
        ProjectKey      = r.GetString(r.GetOrdinal("ProjectKey")),
        Name            = r.GetString(r.GetOrdinal("Name")),
        Organization    = r.IsDBNull(r.GetOrdinal("Organization"))    ? null : r.GetString(r.GetOrdinal("Organization")),
        Visibility      = r.IsDBNull(r.GetOrdinal("Visibility"))      ? null : r.GetString(r.GetOrdinal("Visibility")),
        LastAnalysisDate= r.IsDBNull(r.GetOrdinal("LastAnalysisDate")) ? null : r.GetDateTime(r.GetOrdinal("LastAnalysisDate")),
        CreatedDate     = r.GetDateTime(r.GetOrdinal("CreatedDate"))
    };
}

