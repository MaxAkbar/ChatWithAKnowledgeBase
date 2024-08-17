using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.AI.OpenAI;

namespace ChatWithAKnowledgeBase;
internal class OllamaTextGeneration : ITextGenerator
{
    chatHelper chatHelper;
    private string _modelName;

    public OllamaTextGeneration(HttpClient ollamaClient, string modelName)
    {
        chatHelper = new chatHelper(ollamaClient);
        _modelName = modelName;
    }
    public int MaxTokenTotal => 4096;

    public int CountTokens(string text)
    {
        return DefaultGPTTokenizer.StaticCountTokens(text);
    }

    public async IAsyncEnumerable<string> GenerateTextAsync(string prompt, TextGenerationOptions options, CancellationToken cancellationToken = default)
    {
        await foreach (var response in chatHelper.ChatCompletion(prompt, _modelName, cancellationToken))
        {
            yield return response;
        }
    }

    public IReadOnlyList<string> GetTokens(string text)
    {
        throw new NotImplementedException();
    }
}