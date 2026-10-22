using System.Text;
using Microsoft.Extensions.AI;

namespace ShopFloorAssistant.Core.Chat;

/// <summary>
/// Turns a follow-up such as "How often do I have to do it?" into a standalone search query
/// ("How often must the hydraulic oil of the HP-250 be changed?"). Without this, retrieval only sees
/// the follow-up and cannot know what "it" refers to.
/// </summary>
public sealed class QuestionRewriter(IChatClient chatClient)
{
    public const string Instructions = """
        Rewrite the user's last question so it can be understood without the conversation.
        - Replace words such as "it", "this", "that machine" with what they refer to.
        - Keep the language of the last question.
        - Keep machine names, document codes, alarm codes and numbers exactly.
        - If the question is already standalone, return it unchanged.
        Reply with the rewritten question only.
        """;

    // A rewrite much longer than this is the model answering instead of rewriting.
    private const int MaxRewriteLength = 300;

    // Long answers in the history add little context for resolving a reference.
    private const int MaxHistoryMessageLength = 400;

    public async Task<string> RewriteAsync(string question, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        if (history.Count == 0)
        {
            return question;
        }

        var conversation = new StringBuilder();
        foreach (var message in history)
        {
            var text = message.Text.Length > MaxHistoryMessageLength ? message.Text[..MaxHistoryMessageLength] + "…" : message.Text;
            conversation.Append(message.Role == ChatRole.User ? "User: " : "Assistant: ").AppendLine(text);
        }

        var response = await chatClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, Instructions),
                new ChatMessage(ChatRole.User, $"Conversation:\n{conversation}\nLast question: {question}"),
            ],
            new ChatOptions { Temperature = 0 },
            cancellationToken);

        var rewritten = response.Text.Trim().Trim('"');
        return rewritten.Length is > 0 and <= MaxRewriteLength ? rewritten : question;
    }
}
