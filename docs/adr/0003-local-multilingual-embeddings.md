# 3. Local multilingual embeddings with bge-m3

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

Machine manuals usually come from the vendor in English, while operators often ask questions in their own language (here: Polish). The embedding model has to place a Polish question close to the matching English paragraph.

Ingestion also embeds every chunk of every document. Doing that through a rate-limited hosted API (see ADR 2) would quickly use up the daily quota.

## Decision

Use **bge-m3** served by Ollama as the default embedding model:

- multilingual and cross-lingual retrieval;
- about 1.2 GB, so it fits on a 6 GB GPU together with a small chat model;
- runs locally, with no quota and no data leaving the machine.

## Consequences

- Ollama is required for ingestion in the default setup.
- Vectors have 1024 dimensions. Changing the embedding model later means re-indexing all documents, so the model name is stored with the index.
- Retrieval quality for Polish questions will be measured once the evaluation set exists, and this decision can be revisited then.
