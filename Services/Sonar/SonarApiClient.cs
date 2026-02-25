using System.Net.Http.Headers;
using System.Text.Json;
using BugPredictionBackend.Configurations;
using BugPredictionBackend.Models.Sonar;
using Microsoft.Extensions.Options;

namespace BugPredictionBackend.Services.Sonar;

public class SonarApiClient(IHttpClientFactory httpClientFactory, IOptions<SonarSettings> sonarOptions, ILogger<SonarApiClient> logger)
{
    private readonly SonarSettings _settings = sonarOptions.Value;

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string MetricKeys = "bugs,vulnerabilities,code_smells,coverage,duplicated_lines_density,security_rating,reliability_rating,sqale_rating";
    private const string ModuleMetricKeys = "bugs,vulnerabilities,code_smells,coverage,duplicated_lines_density,complexity,ncloc";

    private HttpClient CreateClient()
    {
        HttpClient client = httpClientFactory.CreateClient("SonarCloud");
        client.BaseAddress = new Uri(_settings.BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);
        return client;
    }

    private async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Sonar API call failed: {Endpoint} | Status: {Status}", endpoint, response.StatusCode);
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<List<SonarProjectEnt>> GetProjectsAsync()
    {
        string endpoint = $"/api/projects/search?organization={_settings.Organization}&ps=500";
        SonarProjectSearchResponseEnt? result = await GetAsync<SonarProjectSearchResponseEnt>(endpoint);
        return result?.Components ?? [];
    }

    public async Task<List<SonarBranchEnt>> GetBranchesAsync(string projectKey)
    {
        string endpoint = $"/api/project_branches/list?project={projectKey}";
        SonarBranchResponseEnt? result = await GetAsync<SonarBranchResponseEnt>(endpoint);
        return result?.Branches ?? [];
    }

    public async Task<Dictionary<string, string>> GetMetricsAsync(string projectKey)
    {
        string endpoint = $"/api/measures/component?component={projectKey}&metricKeys={MetricKeys}";
        SonarMeasureResponseEnt? result = await GetAsync<SonarMeasureResponseEnt>(endpoint);
        return result?.Component?.Measures
            .Where(m => m.Value != null)
            .ToDictionary(m => m.Metric, m => m.Value!) ?? [];
    }

    public async Task<SonarIssueResponseEnt?> GetSeverityAsync(string projectKey)
    {
        string endpoint = $"/api/issues/search?componentKeys={projectKey}&severities=BLOCKER,CRITICAL,MAJOR,MINOR,INFO&ps=1&facets=severities,types";
        return await GetAsync<SonarIssueResponseEnt>(endpoint);
    }

    public async Task<List<SonarAnalysisEnt>> GetScanHistoryAsync(string projectKey)
    {
        string endpoint = $"/api/project_analyses/search?project={projectKey}&ps=100";
        SonarAnalysisResponseEnt? result = await GetAsync<SonarAnalysisResponseEnt>(endpoint);
        return result?.Analyses ?? [];
    }

    public async Task<List<SonarComponentEnt>> GetModuleMetricsAsync(string projectKey)
    {
        string endpoint = $"/api/measures/component_tree?component={projectKey}&metricKeys={ModuleMetricKeys}&ps=500";
        SonarComponentTreeResponseEnt? result = await GetAsync<SonarComponentTreeResponseEnt>(endpoint);
        return result?.Components ?? [];
    }

    public async Task<SonarQualityGateResponseEnt?> GetQualityGateAsync(string projectKey)
    {
        string endpoint = $"/api/qualitygates/project_status?projectKey={projectKey}";
        return await GetAsync<SonarQualityGateResponseEnt>(endpoint);
    }
}
