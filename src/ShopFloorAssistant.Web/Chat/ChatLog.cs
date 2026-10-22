namespace ShopFloorAssistant.Web.Chat;

internal static partial class ChatLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Answering failed for a question of {Length} characters")]
    public static partial void AnswerFailed(ILogger logger, Exception exception, int length);
}
