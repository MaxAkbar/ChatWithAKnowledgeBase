using Microsoft.KernelMemory.Prompts;

namespace ChatWithAKnowledgeBase;
internal class OllamaPromptProvider : IPromptProvider
{
    private const string VerificationPrompt = """
                                            Facts:
                                            {{$facts}}
                                            
                                            Rules:
                                            DO NOT provide an answer beyond the facts.
                                            If you DON'T have sufficient information to provide a response, you must reply with 'INFO NOT FOUND'
                                            Provide an answer given only the facts and rules above, you must comply.

                                            Question: {{$input}}
                                          """;

#pragma warning disable KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly EmbeddedPromptProvider _fallbackProvider = new();
#pragma warning restore KMEXP00 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public string ReadPrompt(string promptName)
    {
        switch (promptName)
        {
            case "answer-with-facts":
                return VerificationPrompt;

            default:
                // Fall back to the default
                return _fallbackProvider.ReadPrompt(promptName);
        }
    }
}