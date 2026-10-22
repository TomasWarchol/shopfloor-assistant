using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.AI;

namespace ShopFloorAssistant.Core.Tests.AI;

public class AiServiceCollectionExtensionsTests
{
    [Fact]
    public void AddShopFloorAi_WithoutConfiguration_UsesLocalOllamaModels()
    {
        using var services = BuildServices(new());

        var chat = services.GetRequiredService<IChatClient>().GetService<ChatClientMetadata>();
        var embeddings = services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
            .GetService<EmbeddingGeneratorMetadata>();

        Assert.NotNull(chat);
        Assert.Equal("qwen3:4b-instruct", chat.DefaultModelId);
        Assert.Equal(new Uri("http://localhost:11434"), chat.ProviderUri);
        Assert.NotNull(embeddings);
        Assert.Equal("bge-m3", embeddings.DefaultModelId);
    }

    [Fact]
    public void AddShopFloorAi_WithGitHubModels_UsesConfiguredModelAndEndpoint()
    {
        using var services = BuildServices(new()
        {
            ["AI:Chat:Provider"] = "GitHubModels",
            ["AI:Chat:Model"] = "openai/gpt-4.1-mini",
            ["AI:GitHubModels:ApiKey"] = "test-token",
        });

        var chat = services.GetRequiredService<IChatClient>().GetService<ChatClientMetadata>();

        Assert.NotNull(chat);
        Assert.Equal("openai/gpt-4.1-mini", chat.DefaultModelId);
        Assert.Equal("models.github.ai", chat.ProviderUri?.Host);
    }

    [Fact]
    public void AddShopFloorAi_WithGitHubModelsAndNoToken_FailsWithSetupHint()
    {
        using var services = BuildServices(new()
        {
            ["AI:Chat:Provider"] = "GitHubModels",
            ["AI:Chat:Model"] = "openai/gpt-4.1-mini",
        });

        var exception = Assert.Throws<OptionsValidationException>(
            () => services.GetRequiredService<IOptions<AiOptions>>().Value);

        Assert.Contains("dotnet user-secrets set", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddShopFloorAi_WithFoundryAndNoEndpoint_FailsValidation()
    {
        using var services = BuildServices(new()
        {
            ["AI:Chat:Provider"] = "Foundry",
            ["AI:Chat:Model"] = "gpt-4.1-mini",
            ["AI:Foundry:ApiKey"] = "test-key",
        });

        var exception = Assert.Throws<OptionsValidationException>(
            () => services.GetRequiredService<IOptions<AiOptions>>().Value);

        Assert.Contains("AI:Foundry:Endpoint", exception.Message, StringComparison.Ordinal);
    }

    // Talks to a real model, so it only runs on request: dotnet test -- --explicit only
    [Fact(Explicit = true)]
    public async Task LocalOllama_AnswersAPrompt()
    {
        using var services = BuildServices(new());
        var chat = services.GetRequiredService<IChatClient>();

        var response = await chat.GetResponseAsync(
            "Reply with exactly one word: ready", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(response.Text));
    }

    private static ServiceProvider BuildServices(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddShopFloorAi(configuration).BuildServiceProvider();
    }
}
