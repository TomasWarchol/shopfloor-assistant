# 7. Rewrite follow-up questions before searching

- **Status:** Accepted
- **Date:** 2026-10-22

## Context

ADR 6 left a gap: retrieval saw only the latest question. In a conversation, operators ask follow-ups such as "And what is the normal temperature?" or "Jak często trzeba to robić?" ("How often do I have to do it?"). These name no machine or task, so the embedding of the follow-up alone matches loosely.

## Decision

When the conversation has history, `QuestionRewriter` makes one extra model call that turns the follow-up into a standalone question. The prompt tells the model to:

- replace references such as "it" with what they refer to;
- keep the language;
- keep codes and numbers exactly.

`ShopFloorChat` then works like this:

- **Search** uses the rewrite.
- **The answer** still gets the original question and the history, so it reads naturally in the conversation.
- **The UI** shows "Searched for: ..." whenever the rewrite differs, so the user can see what was looked up.

Guards:

- The first question of a conversation is never rewritten, so there is no extra call.
- An empty reply, or a reply longer than 300 characters (the model answering instead of rewriting), falls back to the original question.
- Rewriting can be turned off with `Assistant:RewriteFollowUps`.

## Measurements (bge-m3, sample documents)

| Follow-up | Searched as typed | Searched as rewritten |
|---|---|---|
| "And what is the normal temperature?" (after a question about alarm E214) | technical data chunk at rank 3 | rank 1 ("What is the normal operating oil temperature of the HP-250 press?") |
| "Jak często trzeba to robić?" (after "how do I change the hydraulic oil") | rank 1, cosine distance 0.43, a quality instruction among the top 5 | rank 1, distance 0.26, all top 5 from the press manual |

With 4 documents about one press, follow-ups often still land somewhere near the right chunk. The rewrite mainly makes retrieval more confident and less noisy, and the benefit grows with the number of documents. `AnswerEvaluationTests.Answer_FollowUpQuestion_IsUnderstoodFromTheConversation` checks the whole flow with the local model. The rewrite was "Jak często trzeba wymieniać olej hydrauliczny w prasie HP-250?" and the answer "Co 4000 godzin pracy lub raz rocznie [1]".

## Consequences

- A follow-up costs one extra small model call, about 1 s on the local GPU.
- A wrong rewrite can lead the search astray. The "Searched for" line makes that visible to the user.
