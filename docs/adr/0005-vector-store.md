# 5. SQLite by default, Qdrant optional, behind Microsoft.Extensions.VectorData

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

The ingestion worker writes the index and the web app reads it, so the vector store must be persistent and shared between processes.

The first plan was Qdrant in a container started by Aspire. On the development machine Docker could not start its engine (no WSL distribution), and the same would happen to anyone cloning the repo without a working Docker. The project should run with only the .NET SDK and Ollama installed.

## Decision

- The application uses the `VectorStore` abstraction from **Microsoft.Extensions.VectorData**. `AddShopFloorVectorStore()` registers the implementation selected in the `VectorStore` section.
- **Default: SQLite with the sqlite-vec extension**, a single file in `.data/`. Nothing to install.
- **Optional: Qdrant**, started by Aspire as a persistent container: `aspire run -- VectorStore:Provider=Qdrant`.
- Connectors come from **CommunityToolkit.VectorData**.

The Semantic Kernel connectors (`Microsoft.SemanticKernel.Connectors.*` 1.74.0-preview) were tried first. They target VectorData 10.1, and with VectorData 10.11 they fail at runtime in `SearchAsync` (`MissingMethodException: VectorSearchOptions.get_OldFilter`). This was found with a small probe before any feature depended on it. The Community Toolkit connectors target VectorData 10.8 and pass the same probe.

The distance function depends on the store: SQLite supports cosine distance (lower is closer), Qdrant cosine similarity (higher is closer). `VectorStoreOptions.DistanceFunction` keeps that knowledge in one place.

## Consequences

- `git clone` + `aspire run` works without Docker.
- The SQLite connector is still a preview package. That is acceptable for local use; a production deployment would use Qdrant or Azure AI Search through the same abstraction.
- Integration tests run against a real SQLite file and an in-memory store, without containers.
- Collections are named after the embedding model (`chunks-bge-m3`), see ADR 3.
