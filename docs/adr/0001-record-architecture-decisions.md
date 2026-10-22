# 1. Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

This project makes several non-obvious choices (model providers, embeddings, vector store, chunking). Without a record, the reasons get lost and the same questions come back in reviews.

## Decision

Significant decisions are recorded as short Architecture Decision Records in `docs/adr/`, numbered sequentially, using the format: Context, Decision, Consequences.

## Consequences

- Every important trade-off has one place where it is explained.
- A superseded decision is not deleted. It is marked as superseded and links to the new ADR.
