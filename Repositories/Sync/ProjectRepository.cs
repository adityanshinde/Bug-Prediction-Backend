using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Sync;

public class ProjectRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<int> InsertOrUpdateAsync(ProjectEnt entity)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_InsertOrUpdateProject", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@ProjectKey", entity.ProjectKey);
        cmd.Parameters.AddWithValue("@Name", entity.Name);
        cmd.Parameters.AddWithValue("@Organization", (object?)entity.Organization ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Visibility", (object?)entity.Visibility ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LastAnalysisDate", (object?)entity.LastAnalysisDate ?? DBNull.Value);

        await con.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<ProjectEnt>> GetAllAsync()
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetAllProjects", con);
        cmd.CommandType = CommandType.StoredProcedure;

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<ProjectEnt> projects = [];
        while (await reader.ReadAsync())
        {
            projects.Add(MapProject(reader));
        }
        return projects;
    }

    public async Task<ProjectEnt?> GetByIdAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetProjectById", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapProject(reader) : null;
    }

    private static ProjectEnt MapProject(SqlDataReader r) => new()
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
