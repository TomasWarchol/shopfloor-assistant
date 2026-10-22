namespace ShopFloorAssistant.Core.Chat;

/// <summary>Configuration bound from the <c>Assistant</c> section.</summary>
public sealed class AssistantOptions
{
    public const string SectionName = "Assistant";

    /// <summary>Chunks given to the model. Five chunks of 500 characters are about 700 tokens.</summary>
    public int SourceCount { get; set; } = 5;

    /// <summary>Earlier messages sent with a question, so follow-up questions have context.</summary>
    public int HistoryMessages { get; set; } = 6;

    /// <summary>Low temperature: answers should repeat the documents, not be creative.</summary>
    public float Temperature { get; set; } = 0.1f;
}
