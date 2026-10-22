# 4. Page-aware PDF chunking

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

Every answer must cite a document and a page, so operators can check it. Plant documents are PDFs with running headers and footers, tables (alarm codes, technical data) and numbered procedures. Plain text extraction loses this structure:

- headers and footers are repeated in every chunk;
- table cells come out one by one, so "E214" is separated from "Oil temperature high";
- layout analysis puts list numbers in blocks of their own, so "8." is separated from the text of step 8.

## Decision

**Reading** (`PdfDocumentReader`, PdfPig layout analysis):

- Blocks in the top and bottom 8 % of the page are treated as running headers and footers and dropped.
- Blocks that start at the same height are one table row and are joined with `|`: `E214 | Oil temperature high (above 60 °C) | ...`.
- List markers are matched to their text line by vertical center, including markers that layout analysis merged into the text block.

**Chunking** (`DocumentChunker`):

- A chunk never crosses a page, so a citation is always one page.
- A numbered heading (`4.3 Shutdown`) starts a new chunk, and the heading path (`4 Start-up and shutdown > 4.3 Shutdown`) is stored with the chunk.
- The embedding text is `title + section + text`, so a short table row still carries the context it belongs to.
- Chunk ids are derived from document, page and position, and re-ingesting a document replaces its old chunks.

**Chunk size: 500 characters.** Measured with `RetrievalEvaluationTests`: 8 Polish and English questions over the sample documents, bge-m3 embeddings.

| Max chunk length | Chunks | Right chunk first | Right chunk in top 3 | MRR |
|---|---|---|---|---|
| 1200 | 42 | 4 / 8 | 88 % | 0.68 |
| 800 | 44 | 6 / 8 | 88 % | 0.83 |
| **500** | 49 | **7 / 8** | **100 %** | **0.92** |

Long chunks dilute a single fact. For example, "180 Nm" sits in a 10-step procedure. Smaller chunks also fit the 8K input limit of GitHub Models.

## Alternatives considered

- **Microsoft.Extensions.DataIngestion.** Still in preview. A small own pipeline is easier to test and to explain; to be revisited when it reaches GA.
- **Fixed-size chunks with overlap.** Simpler, but they cut procedures and tables at arbitrary places and lose the section context.

## Consequences

- Heading detection relies on numbered headings, which is typical for SOPs and manuals. Documents with unnumbered headings would need a font-size heuristic.
- A table longer than one chunk is split, and only the first part has the header row.
- The thresholds in `RetrievalEvaluationTests` (hit@3 ≥ 85 %, MRR ≥ 0.80) guard against regressions when chunking changes.
