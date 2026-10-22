var builder = DistributedApplication.CreateBuilder(args);

var repositoryRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

var documentsPath = Path.Combine(repositoryRoot, "samples", "documents");

var ingestion = builder.AddProject<Projects.ShopFloorAssistant_Ingestion>("ingestion")
    .WithEnvironment("Ingestion__DocumentsPath", documentsPath);

// The web app serves the same PDFs, so citation links can open the cited page.
var web = builder.AddProject<Projects.ShopFloorAssistant_Web>("web")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Ingestion__DocumentsPath", documentsPath)
    .WaitForCompletion(ingestion);

// SQLite needs nothing installed. Qdrant runs in Docker: aspire run -- VectorStore:Provider=Qdrant
if (builder.Configuration["VectorStore:Provider"] == "Qdrant")
{
    // Persistent container and volume: the index survives restarts of the AppHost.
    var qdrant = builder.AddQdrant("qdrant")
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

    ingestion.WithReference(qdrant).WaitFor(qdrant).WithEnvironment("VectorStore__Provider", "Qdrant");
    web.WithReference(qdrant).WithEnvironment("VectorStore__Provider", "Qdrant");
}
else
{
    var databasePath = Path.Combine(repositoryRoot, ".data", "shopfloor-assistant.db");
    ingestion.WithEnvironment("VectorStore__Provider", "Sqlite").WithEnvironment("VectorStore__SqlitePath", databasePath);
    web.WithEnvironment("VectorStore__Provider", "Sqlite").WithEnvironment("VectorStore__SqlitePath", databasePath);
}

builder.Build().Run();
