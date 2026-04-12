using System.Data;
using Npgsql;
using BugPredictionBackend.Models.DTOs;

namespace BugPredictionBackend.Repositories.Read;

public class MetricsReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<MetricsKpisDto?> GetKpisAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getmetricskpis(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        return await reader.ReadAsync() ? new MetricsKpisDto
        {
            Bugs       = reader.IsDBNull(reader.GetOrdinal("Bugs"))       ? 0 : reader.GetInt32(reader.GetOrdinal("Bugs")),
            CodeSmells = reader.IsDBNull(reader.GetOrdinal("CodeSmells")) ? 0 : reader.GetInt32(reader.GetOrdinal("CodeSmells")),
            Coverage   = reader.IsDBNull(reader.GetOrdinal("Coverage"))   ? 0 : reader.GetDecimal(reader.GetOrdinal("Coverage")),
            Duplication= reader.IsDBNull(reader.GetOrdinal("Duplication"))? 0 : reader.GetDecimal(reader.GetOrdinal("Duplication"))
        } : null;
    }

    public async Task<List<CoverageTrendPointDto>> GetCoverageTrendAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getcoveragehistory(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<CoverageTrendPointDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new CoverageTrendPointDto
            {
                Date     = reader.GetDateTime(reader.GetOrdinal("Date")),
                Coverage = reader.IsDBNull(reader.GetOrdinal("Coverage")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Coverage"))
            });
        }
        return list;
    }

    public async Task<List<BugsVsVulnerabilitiesPointDto>> GetBugsVsVulnerabilitiesAsync(int projectId)
    {
        using NpgsqlConnection con = new(_connectionString);
        using NpgsqlCommand cmd = new("SELECT * FROM dbo.sp_getbugsvulnerabilitieshistory(@p_projectid)", con);
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("@p_projectid", projectId);

        await con.OpenAsync();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<BugsVsVulnerabilitiesPointDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new BugsVsVulnerabilitiesPointDto
            {
                Date            = reader.GetDateTime(reader.GetOrdinal("Date")),
                Bugs            = reader.IsDBNull(reader.GetOrdinal("Bugs"))            ? 0 : reader.GetInt32(reader.GetOrdinal("Bugs")),
                Vulnerabilities = reader.IsDBNull(reader.GetOrdinal("Vulnerabilities")) ? 0 : reader.GetInt32(reader.GetOrdinal("Vulnerabilities"))
            });
        }
        return list;
    }
}

