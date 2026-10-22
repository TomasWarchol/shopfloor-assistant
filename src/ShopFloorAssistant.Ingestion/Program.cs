// Runs once: indexes the PDFs into the vector store and exits. Aspire starts the web app after it completes.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.AI;
using ShopFloorAssistant.Core.Ingestion;
using ShopFloorAssistant.Core.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddShopFloorVectorStore();
builder.Services.AddShopFloorAi(builder.Configuration);
builder.Services.AddShopFloorIngestion(builder.Configuration);

using var host = builder.Build();
await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<IngestionOptions>>().Value;
var summary = await host.Services.GetRequiredService<DocumentIngestor>().IngestDirectoryAsync(options.DocumentsPath);
Log.IngestionCompleted(logger, summary.DocumentCount, summary.ChunkCount, summary.CollectionName);

await host.StopAsync();

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Indexed {DocumentCount} documents into {ChunkCount} chunks in collection {CollectionName}")]
    public static partial void IngestionCompleted(ILogger logger, int documentCount, int chunkCount, string collectionName);
}
