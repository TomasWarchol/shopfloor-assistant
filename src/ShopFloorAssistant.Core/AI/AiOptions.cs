namespace ShopFloorAssistant.Core.AI;

/// <summary>Where a model is hosted. The application code only sees <c>IChatClient</c> / <c>IEmbeddingGenerator</c>.</summary>
public enum AiProvider
{
    /// <summary>Local models served by Ollama. No API key, works offline.</summary>
    Ollama,

    /// <summary>GitHub Models: free, rate-limited access with a GitHub token.</summary>
    GitHubModels,

    /// <summary>Microsoft Foundry deployment exposed through the OpenAI-compatible v1 endpoint.</summary>
    Foundry,
}

/// <summary>Configuration bound from the <c>AI</c> section.</summary>
public sealed class AiOptions
{
    public const string SectionName = "AI";

    public AiModelOptions Chat { get; set; } = new() { Model = "qwen3:4b-instruct" };

    public AiModelOptions Embeddings { get; set; } = new() { Model = "bge-m3" };

    public OllamaOptions Ollama { get; set; } = new();

    public OpenAICompatibleOptions GitHubModels { get; set; } = new() { Endpoint = new Uri("https://models.github.ai/inference") };

    public OpenAICompatibleOptions Foundry { get; set; } = new();
}

public sealed class AiModelOptions
{
    public AiProvider Provider { get; set; } = AiProvider.Ollama;

    /// <summary>Model name (Ollama, GitHub Models) or deployment name (Foundry).</summary>
    public string Model { get; set; } = string.Empty;
}

public sealed class OllamaOptions
{
    public Uri Endpoint { get; set; } = new("http://localhost:11434");
}

public sealed class OpenAICompatibleOptions
{
    public Uri? Endpoint { get; set; }

    /// <summary>Never stored in appsettings.json. Use user-secrets locally and environment variables or Key Vault elsewhere.</summary>
    public string? ApiKey { get; set; }
}
