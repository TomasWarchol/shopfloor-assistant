using System.ClientModel;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

namespace ShopFloorAssistant.Core.AI;

/// <summary>Creates the provider-specific clients behind the Microsoft.Extensions.AI abstractions.</summary>
internal static class AiClientFactory
{
    public static IChatClient CreateChatClient(AiOptions options) => options.Chat.Provider switch
    {
        AiProvider.Ollama => new OllamaApiClient(options.Ollama.Endpoint, options.Chat.Model),
        AiProvider.GitHubModels => CreateOpenAIClient(options.GitHubModels).GetChatClient(options.Chat.Model).AsIChatClient(),
        AiProvider.Foundry => CreateOpenAIClient(options.Foundry).GetChatClient(options.Chat.Model).AsIChatClient(),
        _ => throw new NotSupportedException($"Unknown chat provider '{options.Chat.Provider}'."),
    };

    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(AiOptions options) => options.Embeddings.Provider switch
    {
        AiProvider.Ollama => new OllamaApiClient(options.Ollama.Endpoint, options.Embeddings.Model),
        AiProvider.GitHubModels => CreateOpenAIClient(options.GitHubModels).GetEmbeddingClient(options.Embeddings.Model).AsIEmbeddingGenerator(),
        AiProvider.Foundry => CreateOpenAIClient(options.Foundry).GetEmbeddingClient(options.Embeddings.Model).AsIEmbeddingGenerator(),
        _ => throw new NotSupportedException($"Unknown embeddings provider '{options.Embeddings.Provider}'."),
    };

    // GitHub Models and Foundry both expose an OpenAI-compatible API, so one client type covers them.
    private static OpenAIClient CreateOpenAIClient(OpenAICompatibleOptions options) =>
        new(new ApiKeyCredential(options.ApiKey!), new OpenAIClientOptions { Endpoint = options.Endpoint });
}
