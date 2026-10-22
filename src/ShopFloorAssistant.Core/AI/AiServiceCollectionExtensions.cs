using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ShopFloorAssistant.Core.AI;

public static class AiServiceCollectionExtensions
{
    /// <summary>Name of the ActivitySource and Meter that carry gen_ai traces and token usage metrics.</summary>
    public const string TelemetrySourceName = "ShopFloorAssistant.AI";

    /// <summary>
    /// Registers <see cref="IChatClient"/> and <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> for the
    /// providers selected in the <c>AI</c> configuration section, with OpenTelemetry instrumentation.
    /// </summary>
    public static IServiceCollection AddShopFloorAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AiOptions>, AiOptionsValidator>();

        services.AddChatClient(sp => AiClientFactory.CreateChatClient(sp.GetRequiredService<IOptions<AiOptions>>().Value))
            .UseOpenTelemetry(sourceName: TelemetrySourceName);

        services.AddEmbeddingGenerator(sp => AiClientFactory.CreateEmbeddingGenerator(sp.GetRequiredService<IOptions<AiOptions>>().Value))
            .UseOpenTelemetry(sourceName: TelemetrySourceName);

        return services;
    }
}
