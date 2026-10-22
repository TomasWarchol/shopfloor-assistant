using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Chat;

public static class ChatServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ShopFloorChat"/>. Needs AddShopFloorAi() and AddShopFloorVectorStore().</summary>
    public static IServiceCollection AddShopFloorChat(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AssistantOptions>().Bind(configuration.GetSection(AssistantOptions.SectionName));
        services.AddChunkIndex(configuration);
        services.AddSingleton<DocumentSearch>();
        services.AddSingleton<ShopFloorChat>();
        return services;
    }
}
