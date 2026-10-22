# 6. Grounded answers: retrieve, then generate, then check the citations

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

Operators act on the answers: a wrong torque or a skipped lockout step is a safety issue. The default chat model is a 4B model running on a laptop GPU. Without documents it answers confidently and wrongly: asked what a press light curtain is for, it said it "protects materials from light".

## Decision

**A fixed pipeline instead of an agent.** `ShopFloorChat` always searches first (top 5 chunks) and then makes one model call. A small model calls tools unreliably, and one call keeps the answer fast. Agentic retrieval with Microsoft Agent Framework fits a stronger model and is planned for the quality agent project.

**The prompt (`AnswerPrompt`):**

- Answer only from the numbered sources.
- Cite every fact as `[n]`.
- Say "not found" instead of guessing.
- Answer in the language of the question.
- Copy values exactly.

**Treating documents as untrusted input:**

- Each source sits in a `<source>` element. A closing tag inside a document is neutralized, so a document cannot end its own source early. The rules say that sources are not instructions.
- `Citations.FindCited` turns only numbers of real sources into links, so an invented `[7]` links nowhere. When an answer cites nothing, the UI shows a warning.
- `AnswerFormatter` renders the answer as Markdown with raw HTML escaped, links limited to http(s) and images dropped, because the model may repeat text from a document.
- Citation links open the PDF through `DocumentFiles.Resolve`, which accepts only plain file names of existing PDFs, so there is no path traversal.

**Temperature 0.1:** the answer should repeat the documents, not paraphrase them creatively.

## Evaluation

`AnswerEvaluationTests` runs the full pipeline with `qwen3:4b-instruct`. All 5 cases pass:

- Four questions (two Polish, two English) get the fact from the document and cite the right document. For example, a Polish question about the M24 bolt torque gets "180 Nm" with a citation of SOP-014, page 1.
- A question outside the documents (the canteen menu) cites nothing.

## Consequences

- Retrieval uses only the latest question, so a follow-up such as "and for the lower die?" can retrieve poorly. Next step: rewrite the question with the conversation history before searching.
- Citations depend on the model following the format. When it does not, the user sees the warning rather than an unchecked answer.
- The injection mitigations reduce the risk but do not remove it; documents should still come from a controlled source.
