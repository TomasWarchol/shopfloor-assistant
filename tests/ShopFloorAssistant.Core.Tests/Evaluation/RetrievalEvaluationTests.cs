using System.Globalization;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using OllamaSharp;
using ShopFloorAssistant.Core.Retrieval;
using ShopFloorAssistant.Core.Tests.TestDoubles;

namespace ShopFloorAssistant.Core.Tests.Evaluation;

/// <summary>
/// Measures whether retrieval finds the chunk that contains the answer, with the real embedding model.
/// Needs Ollama with bge-m3, so it only runs on request: dotnet test --explicit only
/// </summary>
public class RetrievalEvaluationTests
{
    // Question, document that answers it, and a fact that must be in the retrieved chunk.
    // Mixed Polish and English questions over mixed Polish and English documents.
    private static readonly (string Question, string Document, string Fact)[] Cases =
    [
        ("What torque do I use for the manual clamping bolts?", "SOP-014", "180 Nm"),
        ("Jaki moment dokręcenia śrub mocujących narzędzie?", "SOP-014", "180 Nm"),
        ("Co zrobić, gdy temperatura oleju przekroczy 60 stopni?", "HP-250", "E214"),
        ("How often must the critical dimensions of part 4471-B be measured?", "WI-203", "co 2 godziny"),
        ("Do czego służy kurtyna świetlna na prasie?", "HP-250", "light curtain on the front side"),
        ("Who may remove a lockout padlock?", "LOTO-001", "removes only their own padlock"),
        ("Kiedy wymienić olej hydrauliczny?", "HP-250", "4000 operating hours"),
        ("What is the die shut height tolerance?", "SOP-014", "±0.1 mm"),
    ];

    [Fact(Explicit = true)]
    public async Task Retrieval_WithBgeM3_FindsTheAnsweringChunk()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        IEmbeddingGenerator<string, Embedding<float>> embeddings = new OllamaApiClient(new Uri("http://localhost:11434"), "bge-m3");
        using var store = new InMemoryVectorStore();
        var summary = await TestIndex.Ingestor(store, embeddings, configuredDimensions: 1024)
            .IngestDirectoryAsync(SampleDocuments.DirectoryPath, cancellationToken);
        var search = new DocumentSearch(embeddings, TestIndex.Chunks(store, dimensions: 1024));

        var ranks = new List<int?>();
        foreach (var (question, document, fact) in Cases)
        {
            var hits = await search.SearchAsync(question, top: 10, cancellationToken);
            var index = hits.ToList().FindIndex(hit =>
                hit.DocumentId.StartsWith(document, StringComparison.Ordinal)
                && hit.Text.Contains(fact, StringComparison.Ordinal));
            ranks.Add(index < 0 ? null : index + 1);
        }

        var hitAt3 = ranks.Count(rank => rank <= 3) / (double)ranks.Count;
        var meanReciprocalRank = ranks.Average(rank => rank is null ? 0 : 1.0 / rank.Value);
        TestContext.Current.SendDiagnosticMessage(
            $"chunks={summary.ChunkCount} hit@3={hitAt3:P0} MRR={meanReciprocalRank:F2} ranks=[{string.Join(", ", ranks.Select(rank => rank?.ToString(CultureInfo.InvariantCulture) ?? "-"))}]");

        Assert.True(hitAt3 >= 0.85, $"hit@3 is {hitAt3:P0}, expected at least 85 %.");
        Assert.True(meanReciprocalRank >= 0.8, $"MRR is {meanReciprocalRank:F2}, expected at least 0.80.");
    }
}
