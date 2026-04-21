using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BugPredictionBackend.Configurations;
using Microsoft.Extensions.Options;

namespace BugPredictionBackend.Services.Read;

public class AiInsightsService(
    SnapshotRepository snapshotRepository,
    MetricsReadRepository metricsReadRepository,
    ModuleRepository moduleRepository,
    SeverityRepository severityRepository,
    IHttpClientFactory httpClientFactory,
    IOptions<AiSettings> aiOptions,
    ILogger<AiInsightsService> logger)
{
    private readonly AiSettings _aiSettings = aiOptions.Value;

    public async Task<AiInsightsDto?> GetInsightsAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        AiInsightsDto result = new();

        // Basic KPIs
        MetricsKpisDto? kpis = await metricsReadRepository.GetKpisAsync(projectId);
        var severity = await severityRepository.GetBySnapshotAsync(snapshot.Id);
        var modules = await moduleRepository.GetBySnapshotAsync(snapshot.Id);
        bool hasCoverageData = (kpis?.Coverage ?? 0) > 0 || modules.Any(m => m.Coverage > 0);

        // Insight: Coverage status
        if (!hasCoverageData)
        {
            result.Insights.Add("Coverage data is unavailable in Sonar for this project. Health score is calculated from issue severity and code quality signals.");
        }
        else if (kpis is not null)
        {
            if (kpis.Coverage < 60)
                result.Insights.Add($"Coverage is low ({kpis.Coverage}%). Consider adding tests to increase coverage.");
            else if (kpis.Coverage < 80)
                result.Insights.Add($"Coverage is moderate ({kpis.Coverage}%). Aim for >80% for better confidence.");
            else
                result.Insights.Add($"Coverage looks healthy ({kpis.Coverage}%).");
        }

        // Insight: Severity
        if (severity is not null)
        {
            if (severity.Critical > 0 || severity.Blocker > 0)
                result.Insights.Add($"High severity issues detected: {severity.Blocker} blocker(s), {severity.Critical} critical(s). Prioritize fixes.");
            else if (severity.Major > 50)
                result.Insights.Add($"Large number of major issues ({severity.Major}). Review recent changes for regressions.");
        }

        // Insight: Top risky modules
        var topRisky = modules
            .OrderByDescending(m => (m.Bugs + m.Vulnerabilities + m.CodeSmells))
            .Take(5)
            .ToList();

        foreach (var m in topRisky)
        {
            result.RiskyModules.Add(new RiskyModuleDto
            {
                ModuleName = m.ModuleName,
                Path = m.Path,
                Bugs = m.Bugs,
                Vulnerabilities = m.Vulnerabilities,
                CodeSmells = m.CodeSmells,
                Coverage = m.Coverage,
                Recommendation = GenerateRecommendation(m)
            });
        }

        result.Score = CalculateHealthScore(kpis, severity, modules, hasCoverageData);

        // Add summary insight
        result.Insights.Insert(0, $"Overall project health score: {result.Score}/100");

        result.Structured = BuildStructuredInsights(result, kpis, severity, hasCoverageData);

        AiStructuredInsightsDto? aiStructured = await TryGenerateAiStructuredInsightsAsync(
            projectId,
            result.Score,
            kpis,
            severity,
            result.RiskyModules);

        if (aiStructured is not null)
        {
            result.Structured = aiStructured;
        }

        return result;
    }

    private static AiStructuredInsightsDto BuildStructuredInsights(
        AiInsightsDto result,
        MetricsKpisDto? kpis,
        SeverityDistributionEnt? severity,
        bool hasCoverageData)
    {
        AiStructuredInsightsDto structured = new()
        {
            ExecutiveSummary = result.Insights.FirstOrDefault() ?? "Project health summary not available.",
            Confidence = "Medium"
        };

        if (kpis is not null && hasCoverageData)
        {
            structured.RiskDrivers.Add(new AiRiskDriverDto
            {
                Title = "Coverage",
                Evidence = $"Current coverage is {kpis.Coverage}%.",
                Impact = kpis.Coverage < 70 ? "Low test coverage increases regression risk." : "Coverage is acceptable."
            });
        }

        if (severity is not null)
        {
            structured.RiskDrivers.Add(new AiRiskDriverDto
            {
                Title = "Issue severity mix",
                Evidence = $"Blocker: {severity.Blocker}, Critical: {severity.Critical}, Major: {severity.Major}",
                Impact = (severity.Blocker + severity.Critical) > 0
                    ? "High-severity issues can impact production stability and security."
                    : "No high-severity issues detected."
            });
        }

        foreach (RiskyModuleDto module in result.RiskyModules.Take(3))
        {
            string moduleName = string.IsNullOrWhiteSpace(module.ModuleName) ? "Unknown module" : module.ModuleName;
            structured.ActionPlan.Add(new AiActionPlanDto
            {
                Priority = (module.Vulnerabilities > 0 || module.Bugs > 10) ? "P1" : "P2",
                Action = $"Reduce risk in {moduleName} by addressing top bugs and code smells.",
                OwnerType = "Backend",
                Effort = module.CodeSmells > 40 ? "High" : "Medium",
                ExpectedImpact = "Improves reliability and lowers defect rate in critical modules."
            });
        }

        if (hasCoverageData)
        {
            structured.QuickWins.AddRange(result.RiskyModules
                .Where(m => m.Coverage < 70)
                .Take(2)
                .Select(m => $"Add focused unit tests for {m.ModuleName ?? "Unknown module"} to raise coverage."));
        }
        else
        {
            structured.QuickWins.Add("Enable test coverage reporting in Sonar to improve score accuracy and recommendation quality.");
        }

        if (!structured.QuickWins.Any())
        {
            structured.QuickWins.Add("No immediate quick wins identified; continue monitoring trend metrics.");
        }

        structured.WatchItems.Add("Track week-over-week changes in blocker and critical issue counts.");
        structured.WatchItems.Add("Monitor top risky modules after each sync run.");
        structured.Assumptions.Add("Insights are based on latest snapshot and available Sonar metrics only.");

        return structured;
    }

    private static int CalculateHealthScore(
        MetricsKpisDto? kpis,
        SeverityDistributionEnt? severity,
        List<ModuleMetricEnt> modules,
        bool hasCoverageData)
    {
        double severityRisk = 0;
        if (severity is not null)
        {
            severityRisk = (severity.Blocker * 12)
                + (severity.Critical * 8)
                + (severity.Major * 4)
                + (severity.Minor * 1)
                + (severity.Info * 0.2);
        }

        int totalBugs = kpis?.Bugs ?? modules.Sum(m => m.Bugs);
        int totalCodeSmells = kpis?.CodeSmells ?? modules.Sum(m => m.CodeSmells);
        int totalVulnerabilities = modules.Sum(m => m.Vulnerabilities);

        double issueRisk = (totalVulnerabilities * 3) + (totalBugs * 2) + (totalCodeSmells * 0.2);

        decimal avgDuplication = modules.Count > 0 ? modules.Average(m => m.Duplication) : (kpis?.Duplication ?? 0);
        double duplicationRisk = avgDuplication > 20 ? (double)((avgDuplication - 20) * 0.5m) : 0;

        double coveragePenalty = 0;
        if (hasCoverageData && kpis is not null)
        {
            coveragePenalty = Math.Clamp((80 - (double)kpis.Coverage) * 0.8, 0, 40);
        }

        double totalRisk = severityRisk + issueRisk + duplicationRisk + coveragePenalty;

        const double normalizationScale = 550d;
        double normalizedPenalty = 100d * (1d - Math.Exp(-totalRisk / normalizationScale));
        int score = 100 - (int)Math.Round(normalizedPenalty);
        return Math.Max(0, score);
    }

    private async Task<AiStructuredInsightsDto?> TryGenerateAiStructuredInsightsAsync(
        int projectId,
        int score,
        MetricsKpisDto? kpis,
        SeverityDistributionEnt? severity,
        List<RiskyModuleDto> riskyModules)
    {
        if (string.IsNullOrWhiteSpace(_aiSettings.GroqApiKey))
        {
            return null;
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_aiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _aiSettings.GroqApiKey);

            string promptPayload = JsonSerializer.Serialize(new
            {
                projectId,
                score,
                kpis,
                severity,
                riskyModules = riskyModules.Take(5)
            });

            var request = new
            {
                model = string.IsNullOrWhiteSpace(_aiSettings.Model) ? "llama-3.1-8b-instant" : _aiSettings.Model,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a senior software quality analyst. Return only JSON with keys: executiveSummary, riskDrivers, actionPlan, quickWins, watchItems, confidence, assumptions. Keep recommendations specific and concise."
                    },
                    new
                    {
                        role = "user",
                        content = $"Analyze this project quality snapshot and produce structured insights: {promptPayload}"
                    }
                }
            };

            StringContent content = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/openai/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AI insights request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseJson);

            if (!document.RootElement.TryGetProperty("choices", out JsonElement choices) || choices.GetArrayLength() == 0)
            {
                return null;
            }

            JsonElement message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("content", out JsonElement messageContentElement))
            {
                return null;
            }

            string? messageContent = messageContentElement.GetString();
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return null;
            }

            AiStructuredInsightsDto? structured = JsonSerializer.Deserialize<AiStructuredInsightsDto>(
                messageContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return NormalizeStructuredInsights(structured);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "AI insights HTTP request failed.");
            return null;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "AI insights response parsing failed.");
            return null;
        }
    }

    private static AiStructuredInsightsDto? NormalizeStructuredInsights(AiStructuredInsightsDto? structured)
    {
        if (structured is null)
        {
            return null;
        }

        structured.ExecutiveSummary ??= string.Empty;
        structured.Confidence = string.IsNullOrWhiteSpace(structured.Confidence) ? "Medium" : structured.Confidence;
        structured.RiskDrivers ??= [];
        structured.ActionPlan ??= [];
        structured.QuickWins ??= [];
        structured.WatchItems ??= [];
        structured.Assumptions ??= [];

        return structured;
    }

    private static string GenerateRecommendation(BugPredictionBackend.Models.Entities.ModuleMetricEnt m)
    {
        List<string> recs = new();
        if (m.Coverage < 70) recs.Add("Increase unit tests for this module");
        if (m.Bugs > 5) recs.Add("Investigate recent bug reports and root causes");
        if (m.Vulnerabilities > 0) recs.Add("Address security vulnerabilities immediately");
        if (m.CodeSmells > 20) recs.Add("Refactor to reduce code smells and complexity");
        return string.Join("; ", recs);
    }
}
