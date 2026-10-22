using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ShopFloorAssistant.Core.Tests.TestDoubles;

/// <summary>Streams a fixed answer word by word and remembers the messages it received.</summary>
internal sealed class FakeChatClient(string answer) : IChatClient
{
    public IReadOnlyList<ChatMessage> ReceivedMessages { get; private set; } = [];

    public ChatOptions? ReceivedOptions { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        ReceivedMessages = [.. messages];
        ReceivedOptions = options;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, answer)));
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
