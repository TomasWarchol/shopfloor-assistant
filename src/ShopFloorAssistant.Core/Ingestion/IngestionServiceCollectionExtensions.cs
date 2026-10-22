using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopFloorAssistant.Core.Storage;

namespace ShopFloorAssistant.Core.Ingestion;

public static class IngestionServiceCollectionExtensions
{
    /// <summary>Registers <see cref="DocumentIngestor"/>. Needs AddShopFloorAi() and AddShopFloorVectorStore().</summary>
    public static IServiceCollection AddShopFloorIngestion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddChunkIndex(configuration);
        services.AddSingleton<DocumentIngestor>();
        return services;
    }

    internal static IServiceCollection AddChunkIndex(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IngestionOptions>().Bind(configuration.GetSection(IngestionOptions.SectionName));
        services.TryAddSingleton<ChunkIndex>();
        return services;
    }
}
