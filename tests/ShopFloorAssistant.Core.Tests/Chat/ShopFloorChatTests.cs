using System.Text;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ShopFloorAssistant.Core.Chat;
using ShopFloorAssistant.Core.Retrieval;
using ShopFloorAssistant.Core.Tests.TestDoubles;

namespace ShopFloorAssistant.Core.Tests.Chat;

public class ShopFloorChatTests
{
    [Fact]
    public async Task AskAsync_SendsTheRetrievedChunksAsNumberedSourcesAndStreamsTheAnswer()
    {
        using var store = new InMemoryVectorStore();
        await TestIndex.Ingestor(store).IngestDirectoryAsync(SampleDocuments.DirectoryPath, TestContext.Current.CancellationToken);
        var model = new FakeChatClient("Tighten the bolts to 180 Nm [1].");
        var chat = CreateChat(store, model, new AssistantOptions { SourceCount = 4 });

        var stream = await chat.AskAsync("How much torque?", [], TestContext.Current.CancellationToken);
        var answer = await ReadAsync(stream);

        Assert.Equal([1, 2, 3, 4], stream.Sources.Select(source => source.Number));
        Assert.All(stream.Sources, source => Assert.Contains(source.Chunk.Text, model.ReceivedMessages[^1].Text, StringComparison.Ordinal));
        Assert.Equal("Tighten the bolts to 180 Nm [1].", answer.Trim());
        Assert.Equal(0.1f, model.ReceivedOptions?.Temperature);
    }

    [Fact]
    public async Task AskAsync_SendsOnlyTheMostRecentHistory()
    {
        using var store = new InMemoryVectorStore();
        var model = new FakeChatClient("Answer.");
        var chat = CreateChat(store, model, new AssistantOptions { HistoryMessages = 2 });
        var history = Enumerable.Range(1, 5).Select(i => new ChatMessage(i % 2 == 1 ? ChatRole.User : ChatRole.Assistant, $"Message {i}")).ToList();

        await ReadAsync(await chat.AskAsync("Follow-up?", history, TestContext.Current.CancellationToken));

        Assert.Equal(["Message 4", "Message 5"], model.ReceivedMessages.Skip(1).SkipLast(1).Select(message => message.Text));
    }

    [Fact]
    public async Task AskAsync_NothingIndexedYet_AsksTheModelWithoutSources()
    {
        using var store = new InMemoryVectorStore();
        var model = new FakeChatClient("I could not find this in the documents.");
        var chat = CreateChat(store, model, new AssistantOptions());

        var stream = await chat.AskAsync("Anything?", [], TestContext.Current.CancellationToken);
        await ReadAsync(stream);

        Assert.Empty(stream.Sources);
        Assert.Contains("none were found", model.ReceivedMessages[^1].Text, StringComparison.Ordinal);
    }

    private static ShopFloorChat CreateChat(InMemoryVectorStore store, FakeChatClient model, AssistantOptions options) => new(
        new DocumentSearch(new FakeEmbeddingGenerator(TestIndex.Dimensions), TestIndex.Chunks(store)),
        model,
        Options.Create(options));

    private static async Task<string> ReadAsync(StreamingAnswer stream)
    {
        var answer = new StringBuilder();
        await foreach (var update in stream.Updates)
        {
            answer.Append(update.Text);
        }

        return answer.ToString();
    }
}
