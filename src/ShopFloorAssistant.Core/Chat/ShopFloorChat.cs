using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Chat;

/// <summary>The sources found for a question and the answer as it streams from the model.</summary>
public sealed record StreamingAnswer(IReadOnlyList<Source> Sources, IAsyncEnumerable<ChatResponseUpdate> Updates);

/// <summary>Retrieval-augmented answers: search the documents first, then let the model answer from them only.</summary>
public sealed class ShopFloorChat(DocumentSearch search, IChatClient chatClient, IOptions<AssistantOptions> options)
{
    private readonly AssistantOptions _options = options.Value;

    /// <param name="history">Earlier questions and answers of this conversation, without their sources.</param>
    public async Task<StreamingAnswer> AskAsync(string question, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        var chunks = await search.SearchAsync(question, _options.SourceCount, cancellationToken);
        var sources = chunks.Select((chunk, index) => new Source(index + 1, chunk)).ToList();

        var messages = AnswerPrompt.Build(question, sources, history.TakeLast(_options.HistoryMessages));
        var updates = chatClient.GetStreamingResponseAsync(
            messages, new ChatOptions { Temperature = _options.Temperature }, cancellationToken);

        return new StreamingAnswer(sources, updates);
    }
}
