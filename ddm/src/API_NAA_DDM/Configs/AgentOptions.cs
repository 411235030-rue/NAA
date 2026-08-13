namespace API_NAA_DDM.Configs;

public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    public string Provider { get; set; } = "Local";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string ChatPath { get; set; } = "chat-messages";
    public int TimeoutSeconds { get; set; } = 120;
}
