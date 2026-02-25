using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Read;

public class ProjectReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<ProjectEnt>> GetAllAsync()
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetAllProjects", con);
        cmd.CommandType = CommandType.StoredProcedure;

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<ProjectEnt> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new ProjectEnt
            {
                Id               = reader.GetInt32(reader.GetOrdinal("Id")),
                ProjectKey       = reader.GetString(reader.GetOrdinal("ProjectKey")),
                Name             = reader.GetString(reader.GetOrdinal("Name")),
                Organization     = reader.IsDBNull(reader.GetOrdinal("Organization"))     ? null : reader.GetString(reader.GetOrdinal("Organization")),
                Visibility       = reader.IsDBNull(reader.GetOrdinal("Visibility"))       ? null : reader.GetString(reader.GetOrdinal("Visibility")),
                LastAnalysisDate = reader.IsDBNull(reader.GetOrdinal("LastAnalysisDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastAnalysisDate")),
                CreatedDate      = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }
        return list;
    }

    public async Task<bool> ExistsAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetProjectById", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }
}
