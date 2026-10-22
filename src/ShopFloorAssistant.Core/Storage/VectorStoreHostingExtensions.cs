using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qdrant.Client;

namespace ShopFloorAssistant.Core.Storage;

public static class VectorStoreHostingExtensions
{
    /// <summary>
    /// Registers the <see cref="Microsoft.Extensions.VectorData.VectorStore"/> selected in the <c>VectorStore</c>
    /// section. The rest of the application only sees the abstraction, as with the AI providers.
    /// </summary>
    public static TBuilder AddShopFloorVectorStore<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(VectorStoreOptions.SectionName);
        builder.Services.Configure<VectorStoreOptions>(section);
        var options = section.Get<VectorStoreOptions>() ?? new VectorStoreOptions();

        switch (options.Provider)
        {
            case VectorStoreProvider.Qdrant:
                // Connection string "qdrant" is provided by Aspire (endpoint and API key).
                builder.AddQdrantClient("qdrant");
                builder.Services.AddQdrantVectorStore(services => services.GetRequiredService<QdrantClient>());
                break;

            default:
                var path = Path.GetFullPath(options.SqlitePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                builder.Services.AddSqliteVectorStore(_ => $"Data Source={path}");
                break;
        }

        return builder;
    }
}
