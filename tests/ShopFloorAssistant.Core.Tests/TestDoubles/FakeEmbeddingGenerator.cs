using Microsoft.Extensions.AI;

namespace ShopFloorAssistant.Core.Tests.TestDoubles;

/// <summary>Deterministic vectors, so the pipeline can be tested without a model.</summary>
internal sealed class FakeEmbeddingGenerator(int dimensions) : IEmbeddingGenerator<string, Embedding<float>>
{
    public static float[] Vector(string value, int dimensions) =>
        [.. Enumerable.Range(1, dimensions).Select(i => (float)((value.Length % 13) + i))];

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var embeddings = values.Select(value => new Embedding<float>(Vector(value, dimensions)));
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
