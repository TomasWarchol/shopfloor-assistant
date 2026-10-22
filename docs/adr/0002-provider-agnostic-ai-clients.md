# 2. Provider-agnostic AI clients with Microsoft.Extensions.AI

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

The assistant has to run in three situations:

1. **Anyone cloning the repo**, with no API keys at all.
2. **Development and demos** with a better model than a laptop can run.
3. **A production-like setup** in Microsoft Foundry.

Hard-coding one SDK would force a choice between these, and model calls would leak provider types into the application code.

## Decision

The application depends only on the `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>` abstractions from **Microsoft.Extensions.AI**. `AddShopFloorAi()` picks the implementation from the `AI` configuration section:

| Provider | Implementation | Typical use |
|---|---|---|
| `Ollama` (default) | `OllamaSharp.OllamaApiClient` | Local, offline, no keys |
| `GitHubModels` | OpenAI .NET client pointed at `https://models.github.ai/inference` | Free, rate-limited access to stronger models |
| `Foundry` | OpenAI .NET client pointed at the resource's `/openai/v1/` endpoint | Production-like deployment |

GitHub Models and Foundry both expose OpenAI-compatible endpoints, so one client type covers them. Chat and embeddings are configured separately, so embeddings can stay local while chat uses a hosted model.

Every client is wrapped with OpenTelemetry (`ShopFloorAssistant.AI` source and meter), so model calls and token usage show up in the Aspire dashboard.

Invalid configuration fails at startup with a message that explains the fix, for example the `dotnet user-secrets` command for a missing token.

## Consequences

- Switching providers is a configuration change, not a code change.
- Unit tests check provider selection without network calls. Tests that need a real model are marked `Explicit`.
- Only features common to all providers are available through the abstraction. Provider-specific features need `GetService<T>()` or a dedicated adapter.
- GitHub Models has daily request limits and around 8K input tokens per request, so the retrieval step has to keep the context small.
