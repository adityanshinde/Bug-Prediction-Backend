using System.Data;
using Microsoft.Data.SqlClient;
using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;

namespace BugPredictionBackend.Repositories.Read;

public class QAReadRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task<List<QualityGateConditionDto>> GetConditionsBySnapshotAsync(int snapshotId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetConditionsBySnapshot", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@SnapshotId", snapshotId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<QualityGateConditionDto> list = [];
        while (await reader.ReadAsync())
        {
            string metricKey   = reader.GetString(reader.GetOrdinal("MetricKey"));
            string? comparator = reader.IsDBNull(reader.GetOrdinal("Comparator"))     ? null : reader.GetString(reader.GetOrdinal("Comparator"));
            string? threshold  = reader.IsDBNull(reader.GetOrdinal("ErrorThreshold")) ? null : reader.GetString(reader.GetOrdinal("ErrorThreshold"));
            string? actual     = reader.IsDBNull(reader.GetOrdinal("ActualValue"))    ? null : reader.GetString(reader.GetOrdinal("ActualValue"));
            string status      = reader.GetString(reader.GetOrdinal("Status"));

            list.Add(new QualityGateConditionDto
            {
                Metric      = metricKey,
                Condition   = BuildConditionLabel(comparator, threshold),
                ActualValue = actual,
                Status      = status == "OK" ? "PASS" : "FAIL"
            });
        }
        return list;
    }

    public async Task<List<QAEntryResponseDto>> GetEntriesAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetManualQAEntries", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        List<QAEntryResponseDto> list = [];
        while (await reader.ReadAsync())
        {
            list.Add(new QAEntryResponseDto
            {
                Id          = reader.GetInt32(reader.GetOrdinal("Id")),
                ProjectId   = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                ModuleName  = reader.GetString(reader.GetOrdinal("ModuleName")),
                IssueType   = reader.GetString(reader.GetOrdinal("IssueType")),
                Severity    = reader.GetString(reader.GetOrdinal("Severity")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                ReportedBy  = reader.IsDBNull(reader.GetOrdinal("ReportedBy"))  ? null : reader.GetString(reader.GetOrdinal("ReportedBy")),
                EntryDate   = reader.GetDateTime(reader.GetOrdinal("EntryDate"))
            });
        }
        return list;
    }

    public async Task<QASummaryDto> GetSummaryAsync(int projectId)
    {
        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new("sp_GetQASummary", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProjectId", projectId);

        await con.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        QASummaryDto summary = new();
        if (await reader.ReadAsync())
        {
            summary.TotalEntries         = reader.IsDBNull(reader.GetOrdinal("TotalEntries"))         ? 0 : reader.GetInt32(reader.GetOrdinal("TotalEntries"));
            summary.BugEntries           = reader.IsDBNull(reader.GetOrdinal("BugEntries"))           ? 0 : reader.GetInt32(reader.GetOrdinal("BugEntries"));
            summary.VulnerabilityEntries = reader.IsDBNull(reader.GetOrdinal("VulnerabilityEntries")) ? 0 : reader.GetInt32(reader.GetOrdinal("VulnerabilityEntries"));
            summary.CodeSmellEntries     = reader.IsDBNull(reader.GetOrdinal("CodeSmellEntries"))     ? 0 : reader.GetInt32(reader.GetOrdinal("CodeSmellEntries"));
            summary.CriticalCount        = reader.IsDBNull(reader.GetOrdinal("CriticalCount"))        ? 0 : reader.GetInt32(reader.GetOrdinal("CriticalCount"));
            summary.HighCount            = reader.IsDBNull(reader.GetOrdinal("HighCount"))            ? 0 : reader.GetInt32(reader.GetOrdinal("HighCount"));
            summary.MediumCount          = reader.IsDBNull(reader.GetOrdinal("MediumCount"))          ? 0 : reader.GetInt32(reader.GetOrdinal("MediumCount"));
            summary.LowCount             = reader.IsDBNull(reader.GetOrdinal("LowCount"))             ? 0 : reader.GetInt32(reader.GetOrdinal("LowCount"));
        }
        return summary;
    }

    // Converts comparator + threshold into a human-readable label e.g. "GT 1" ? "> 1"
    private static string? BuildConditionLabel(string? comparator, string? threshold)
    {
        if (comparator is null || threshold is null) return null;
        return comparator switch
        {
            "GT" => $"> {threshold}",
            "LT" => $"< {threshold}",
            _    => $"{comparator} {threshold}"
        };
    }
}
