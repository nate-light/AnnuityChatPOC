namespace AnnuityChatPOC.Application.Abstractions;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4.1-mini";

    public string Endpoint { get; set; } = "https://api.openai.com/v1/responses";

    public int TimeoutSeconds { get; set; } = 45;
}

