using ChatWithAKnowledgeBase;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using System.Net;
using System.Text;
using System.Text.Json;

var ollamaEndpoint = "http://127.0.0.1:11434";
var ollamaClient = new HttpClient
{
    BaseAddress = new Uri(ollamaEndpoint)
};
var modeName = await SelectOllamaModel(ollamaClient);

// prompt the user if they want to chat with the model or knowledge base
Console.WriteLine("Would you like to chat with the model or a knowledge base?");
Console.WriteLine("(0) Chat with LLM");
Console.WriteLine("(1) Knowledge Base");
Console.Write("User > ");

var userInput = Console.ReadLine();

if (string.IsNullOrWhiteSpace(userInput))
{
    Console.WriteLine("Invalid input.");
}
else if (userInput == "1")
{
    await ImportDocument(ollamaClient, modeName);
}
else if (userInput == "0")
{
    await StartChat(ollamaClient, modeName);
}

Console.WriteLine("Exiting the application.");


static async Task ImportDocument(HttpClient ollamaClient, string modelName)
{
    // ask the user if they want to import a document or a webpage
    Console.WriteLine("What would you like to import?");
    Console.WriteLine("(0) Document");
    Console.WriteLine("(1) Webpage");
    Console.WriteLine("(2) Chat with Knowledge Base");
    Console.Write("User > ");

    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.WriteLine("Invalid input.");

        return;
    }

    var documentText = string.Empty;
    var documentImporter = new DocumentImporter();
    var webPageImporter = new WebPageImporter();

    switch (userInput)
    {
        case "0":
            Console.WriteLine("Please provide file path.");

            var filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Invalid file path.");

                return;
            }

            documentText = await documentImporter.Import(filePath);

            break;
        case "1":
            Console.WriteLine("Please provide web page url.");

            var webUrl = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(webUrl))
            {
                Console.WriteLine("Invalid file path.");

                return;
            }

            documentText = await webPageImporter.Import(webUrl);

            break;
        case "2":
            Console.WriteLine("What would you like to retrieve?"); 
            break;
    }

    var kernelMemory = GetMemoryKernel(ollamaClient, modelName);

    if (string.IsNullOrWhiteSpace(documentText) && !userInput.Equals("2"))
    {
        Console.WriteLine("Failed to import the document.");

        return;
    }
    else
    {
        // import the document into the memory kernel
        var importedText = await kernelMemory.ImportTextAsync(documentText);

        Console.WriteLine("Document imported successfully. What would you like to retrieve?");
    }
    
    Console.Write("User > ");

    var question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
    {
        Console.WriteLine("Invalid input.");

        return;
    }

    var answer = await kernelMemory.AskAsync(question);

    Console.WriteLine(answer);
}

static MemoryServerless GetMemoryKernel(HttpClient ollamaClient, string modelName)
{
    var memoryBuilder = new KernelMemoryBuilder();

    memoryBuilder.WithCustomPromptProvider(new OllamaPromptProvider());
    memoryBuilder.WithCustomEmbeddingGenerator(new OllamaTextEmbedding())
                .WithCustomTextGenerator(new OllamaTextGeneration(ollamaClient, modelName))
                .WithSimpleVectorDb(new SimpleVectorDbConfig { Directory = "VectorDirectory", StorageType = FileSystemTypes.Disk })
                .Build<MemoryServerless>();

    var memory = memoryBuilder.Build<MemoryServerless>();

    return memory;
}

static async Task<bool> ChatWithModel(HttpClient ollamaClient, ChatRequest chatRequest)
{
    Console.Write("User > ");

    var endOfConversation = false;
    var userInput = Console.ReadLine();

    if (userInput == "/bye" || string.IsNullOrWhiteSpace(userInput))
    {
        return endOfConversation = false;
    }

    ChatResponse? chatResponse = await ChatCompletion(ollamaClient, chatRequest, userInput);

    if (chatResponse != null)
    {
        var assistantMessage = new Message { Role = chatResponse.Message.Role, Content = chatResponse.Message.Content };
        chatRequest.Messages.Add(assistantMessage);
        Console.WriteLine($"{assistantMessage.Role} > {assistantMessage.Content}");

        endOfConversation = true;
    }
    else
    {
        Console.WriteLine("Failed to deserialize the response.");

        endOfConversation = false;
    }

    return endOfConversation;
}

static async Task<string> SelectOllamaModel(HttpClient ollamaClient)
{
    var responseMessage = await ollamaClient.GetAsync("/api/tags");
    var content = await responseMessage.Content.ReadAsStringAsync();

    if (responseMessage != null && responseMessage.StatusCode == HttpStatusCode.OK && content != null)
    {
        // Deserialize the JSON string to the ModelsResponse object
        ModelsResponse modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(content)!;

        if (modelsResponse != null)
        {
            // Output the deserialized object
            for (int i = 0; i < modelsResponse.Models.Count; i++)
            {
                Model? model = modelsResponse.Models[i];
                Console.WriteLine($"({i}) {model.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("Please use the numeric value for the model to interact with.");
            Console.Write("User > ");

            var userInput = Console.ReadLine();

            if (!int.TryParse(userInput, out int modelIndex) || modelIndex < 0 || modelIndex >= modelsResponse.Models.Count)
            {
                Console.WriteLine("Invalid model index.");

                return string.Empty;
            }

            return modelsResponse.Models[modelIndex].Name;
        }
    }

    return string.Empty;
}

static async Task StartChat(HttpClient ollamaClient, string modeName)
{
    if (modeName != string.Empty)
    {
        // chat with the model
        Console.Clear();
        Console.WriteLine("Chatting with the model...");
        Console.WriteLine("To end the chat type /bye");
        Console.WriteLine();

        var chatRequest = new ChatRequest
        {
            Model = modeName,
            Messages = [],
            Stream = false
        };
        var userMessage = new Message { Role = "system", Content = "You are a helpfull assistant." };

        chatRequest.Messages.Add(userMessage);

        while (await ChatWithModel(ollamaClient, chatRequest)) ;
    }
}

static async Task<ChatResponse?> ChatCompletion(HttpClient ollamaClient, ChatRequest chatRequest, string userInput)
{
    var userMessage = new Message { Role = "user", Content = userInput };

    chatRequest.Messages.Add(userMessage);

    var chatRequestJson = JsonSerializer.Serialize(chatRequest);
    var content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json");
    var responseMessage = await ollamaClient.PostAsync("/api/chat", content);
    var llmResponse = await responseMessage.Content.ReadAsStringAsync();
    var chatResponse = JsonSerializer.Deserialize<ChatResponse>(llmResponse);
    return chatResponse;
}