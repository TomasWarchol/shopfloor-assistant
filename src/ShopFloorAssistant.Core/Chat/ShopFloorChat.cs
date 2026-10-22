using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Chat;

/// <summary>The sources found for a question and the answer as it streams from the model.</summary>
/// <param name="SearchQuery">What was searched: the question itself, or its standalone rewrite for a follow-up.</param>
public sealed record StreamingAnswer(string SearchQuery, IReadOnlyList<Source> Sources, IAsyncEnumerable<ChatResponseUpdate> Updates);

/// <summary>Retrieval-augmented answers: search the documents first, then let the model answer from them only.</summary>
public sealed class ShopFloorChat(DocumentSearch search, QuestionRewriter rewriter, IChatClient chatClient, IOptions<AssistantOptions> options)
{
    private readonly AssistantOptions _options = options.Value;

    /// <param name="history">Earlier questions and answers of this conversation, without their sources.</param>
    public async Task<StreamingAnswer> AskAsync(string question, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        var recentHistory = history.TakeLast(_options.HistoryMessages).ToList();
        var searchQuery = _options.RewriteFollowUps
            ? await rewriter.RewriteAsync(question, recentHistory, cancellationToken)
            : question;

        var chunks = await search.SearchAsync(searchQuery, _options.SourceCount, cancellationToken);
        var sources = chunks.Select((chunk, index) => new Source(index + 1, chunk)).ToList();

        // The answer still gets the original question and the history, so it reads naturally in the conversation.
        var messages = AnswerPrompt.Build(question, sources, recentHistory);
        var updates = chatClient.GetStreamingResponseAsync(
            messages, new ChatOptions { Temperature = _options.Temperature }, cancellationToken);

        return new StreamingAnswer(searchQuery, sources, updates);
    }
}
