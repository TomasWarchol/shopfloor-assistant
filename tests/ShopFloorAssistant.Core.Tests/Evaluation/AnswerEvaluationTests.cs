using System.Text;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;
using ShopFloorAssistant.Core.Chat;
using ShopFloorAssistant.Core.Retrieval;
using ShopFloorAssistant.Core.Tests.TestDoubles;

namespace ShopFloorAssistant.Core.Tests.Evaluation;

/// <summary>
/// Asks the full pipeline (retrieval and the local chat model) and checks that answers contain the
/// fact from the documents, cite the right document, and admit when the documents have no answer.
/// Needs Ollama with bge-m3 and qwen3:4b-instruct: dotnet test --explicit only
/// </summary>
public class AnswerEvaluationTests
{
    public static TheoryData<string, string, string> AnswerableQuestions => new()
    {
        { "Jaki moment dokręcenia śrub M24 przy mocowaniu narzędzia?", "SOP-014", "180" },
        { "What should I do when alarm E214 appears?", "HP-250", "40 L/min" },
        { "Jaka jest tolerancja średnicy otworu A w detalu 4471-B?", "WI-203", "+0,1" },
        { "Who is allowed to remove a lockout padlock?", "LOTO-001", "own" },
    };

    [Theory(Explicit = true)]
    [MemberData(nameof(AnswerableQuestions))]
    public async Task Answer_ContainsTheFactAndCitesTheDocument(string question, string document, string fact)
    {
        var (answer, cited) = await AskAsync(question);

        Assert.Contains(fact, answer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(cited, source => source.Chunk.DocumentId.StartsWith(document, StringComparison.Ordinal));
    }

    [Fact(Explicit = true)]
    public async Task Answer_QuestionOutsideTheDocuments_CitesNothing()
    {
        var (_, cited) = await AskAsync("What is on the canteen menu today?");

        Assert.Empty(cited);
    }

    private static async Task<(string Answer, IReadOnlyList<Source> Cited)> AskAsync(string question)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var chat = await CreateChatAsync(cancellationToken);

        var stream = await chat.AskAsync(question, [], cancellationToken);
        var answer = new StringBuilder();
        await foreach (var update in stream.Updates)
        {
            answer.Append(update.Text);
        }

        var cited = Citations.FindCited(answer.ToString(), stream.Sources);
        TestContext.Current.TestOutputHelper?.WriteLine($"Q: {question}\nA: {answer}\nSources: {string.Join(", ", stream.Sources.Select(source => $"[{source.Number}] {source.Chunk.DocumentId} p{source.Chunk.PageNumber}"))}\nCited: {string.Join(", ", cited.Select(source => source.Number))}");
        return (answer.ToString(), cited);
    }

    private static async Task<ShopFloorChat> CreateChatAsync(CancellationToken cancellationToken)
    {
        var ollama = new Uri("http://localhost:11434");
        IEmbeddingGenerator<string, Embedding<float>> embeddings = new OllamaApiClient(ollama, "bge-m3");
        var store = new InMemoryVectorStore();
        await TestIndex.Ingestor(store, embeddings, configuredDimensions: 1024).IngestDirectoryAsync(SampleDocuments.DirectoryPath, cancellationToken);

        return new ShopFloorChat(
            new DocumentSearch(embeddings, TestIndex.Chunks(store, dimensions: 1024)),
            new OllamaApiClient(ollama, "qwen3:4b-instruct"),
            Options.Create(new AssistantOptions()));
    }
}
