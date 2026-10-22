using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShopFloorAssistant.Core.Ingestion;

public static class IngestionServiceCollectionExtensions
{
    /// <summary>Registers <see cref="DocumentIngestor"/>. Needs AddShopFloorAi() and a VectorStore registration.</summary>
    public static IServiceCollection AddShopFloorIngestion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IngestionOptions>().Bind(configuration.GetSection(IngestionOptions.SectionName));
        services.AddSingleton<DocumentIngestor>();
        return services;
    }
}
