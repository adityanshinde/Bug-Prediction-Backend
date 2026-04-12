using System.Data;
using Npgsql;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Read;

public class ProjectReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<ProjectEnt>> GetAllAsync()
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getallprojects()", con);
        cmd.CommandType = CommandType.Text;

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

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
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getprojectbyid(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }
}

