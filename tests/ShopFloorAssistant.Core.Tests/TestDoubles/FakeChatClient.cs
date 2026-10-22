using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ShopFloorAssistant.Core.Tests.TestDoubles;

/// <summary>
/// Streams a fixed answer word by word, returns a fixed text for non-streaming calls (used by the question
/// rewriter), and remembers what each kind of call received.
/// </summary>
internal sealed class FakeChatClient(string answer, string? response = null) : IChatClient
{
    /// <summary>Messages of the last streaming call: the answer prompt.</summary>
    public IReadOnlyList<ChatMessage> ReceivedMessages { get; private set; } = [];

    public ChatOptions? ReceivedOptions { get; private set; }

    /// <summary>Messages of every non-streaming call: the rewrite prompts.</summary>
    public List<IReadOnlyList<ChatMessage>> ResponseRequests { get; } = [];

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ResponseRequests.Add([.. messages]);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response ?? answer)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ReceivedMessages = [.. messages];
        ReceivedOptions = options;
        foreach (var word in answer.Split(' '))
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, word + " ");
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
