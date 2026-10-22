using System.Globalization;
using System.Text.RegularExpressions;
using ShopFloorAssistant.Core.Retrieval;

namespace ShopFloorAssistant.Core.Chat;

/// <summary>A retrieved chunk as the model sees it: [1], [2], ...</summary>
public sealed record Source(int Number, RetrievedChunk Chunk);

public static partial class Citations
{
    /// <summary>
    /// The sources the answer actually cites, in the order of first mention. Accepts "[2]", "[1][3]" and "[1, 3]".
    /// Numbers that do not match a source are ignored, so an invented "[7]" never becomes a link.
    /// </summary>
    public static IReadOnlyList<Source> FindCited(string answer, IReadOnlyList<Source> sources) =>
        [.. CitationMarker().Matches(answer)
            .SelectMany(match => match.Groups[1].Value.Split([',', ';'], StringSplitOptions.TrimEntries))
            .Select(number => int.Parse(number, CultureInfo.InvariantCulture))
            .Distinct()
            .Select(number => sources.FirstOrDefault(source => source.Number == number))
            .OfType<Source>()];

    [GeneratedRegex(@"\[(\d{1,2}(?:\s*[,;]\s*\d{1,2})*)\]")]
    internal static partial Regex CitationMarker();
}
