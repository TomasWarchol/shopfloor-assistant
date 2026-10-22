using Microsoft.Extensions.AI;
using ShopFloorAssistant.Core.Chat;
using ShopFloorAssistant.Core.Tests.TestDoubles;

namespace ShopFloorAssistant.Core.Tests.Chat;

public class QuestionRewriterTests
{
    private static readonly ChatMessage[] History =
    [
        new(ChatRole.User, "How do I change the hydraulic oil of the HP-250?"),
        new(ChatRole.Assistant, "Apply lockout/tagout, drain through valve V-7, refill 900 L of HLP 46 [1]."),
    ];

    [Fact]
    public async Task RewriteAsync_FirstQuestion_ReturnsItWithoutCallingTheModel()
    {
        var model = new FakeChatClient("unused");

        var query = await new QuestionRewriter(model).RewriteAsync("What is E214?", [], TestContext.Current.CancellationToken);

        Assert.Equal("What is E214?", query);
        Assert.Empty(model.ResponseRequests);
    }

    [Fact]
    public async Task RewriteAsync_FollowUp_SendsTheConversationAndReturnsTheRewrite()
    {
        var model = new FakeChatClient("unused", response: "\"How often must the hydraulic oil of the HP-250 be changed?\"\n");

        var query = await new QuestionRewriter(model).RewriteAsync("How often?", History, TestContext.Current.CancellationToken);

        Assert.Equal("How often must the hydraulic oil of the HP-250 be changed?", query);
        var prompt = Assert.Single(model.ResponseRequests);
        Assert.Equal(QuestionRewriter.Instructions, prompt[0].Text);
        Assert.Contains("User: How do I change the hydraulic oil of the HP-250?", prompt[1].Text, StringComparison.Ordinal);
        Assert.EndsWith("Last question: How often?", prompt[1].Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RewriteAsync_EmptyRewrite_FallsBackToTheQuestion(string rewrite)
    {
        var model = new FakeChatClient("unused", response: rewrite);

        Assert.Equal("How often?", await new QuestionRewriter(model).RewriteAsync("How often?", History, TestContext.Current.CancellationToken));
    }

    // A long reply means the model answered the question instead of rewriting it.
    [Fact]
    public async Task RewriteAsync_ModelAnswersInsteadOfRewriting_FallsBackToTheQuestion()
    {
        var model = new FakeChatClient("unused", response: string.Join(' ', Enumerable.Repeat("The oil is changed every 4000 hours.", 20)));

        Assert.Equal("How often?", await new QuestionRewriter(model).RewriteAsync("How often?", History, TestContext.Current.CancellationToken));
    }
}
