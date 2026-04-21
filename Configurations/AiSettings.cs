namespace BugPredictionBackend.Configurations;

public class AiSettings
{
    public string BaseUrl { get; set; } = "https://api.groq.com";
    public string Model { get; set; } = "llama-3.1-8b-instant";
    public string GroqApiKey { get; set; } = string.Empty;
}
