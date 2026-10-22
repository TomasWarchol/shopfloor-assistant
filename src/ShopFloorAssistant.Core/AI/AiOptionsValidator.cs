using Microsoft.Extensions.Options;

namespace ShopFloorAssistant.Core.AI;

/// <summary>Fails fast at startup with a message that says how to fix the configuration.</summary>
internal sealed class AiOptionsValidator : IValidateOptions<AiOptions>
{
    public ValidateOptionsResult Validate(string? name, AiOptions options)
    {
        var failures = new List<string>();

        ValidateModel("AI:Chat", options.Chat, options, failures);
        ValidateModel("AI:Embeddings", options.Embeddings, options, failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateModel(string section, AiModelOptions model, AiOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(model.Model))
        {
            failures.Add($"{section}:Model is empty.");
        }

        switch (model.Provider)
        {
            case AiProvider.GitHubModels when string.IsNullOrWhiteSpace(options.GitHubModels.ApiKey):
                failures.Add(
                    $"{section} uses GitHub Models, but AI:GitHubModels:ApiKey is not set. Create a token with the " +
                    "'models:read' permission and run: dotnet user-secrets set \"AI:GitHubModels:ApiKey\" \"<token>\" " +
                    "--project src/ShopFloorAssistant.Web");
                break;

            case AiProvider.Foundry when options.Foundry.Endpoint is null || string.IsNullOrWhiteSpace(options.Foundry.ApiKey):
                failures.Add(
                    $"{section} uses Microsoft Foundry, but AI:Foundry:Endpoint or AI:Foundry:ApiKey is not set. " +
                    "Use the OpenAI v1 endpoint of your resource, e.g. https://<resource>.openai.azure.com/openai/v1/");
                break;
        }
    }
}
