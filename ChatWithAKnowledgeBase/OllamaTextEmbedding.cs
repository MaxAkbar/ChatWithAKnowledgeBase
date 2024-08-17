using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.AI.OpenAI;
using Newtonsoft.Json;
using System.Text;

namespace ChatWithAKnowledgeBase;
internal class OllamaTextEmbedding : ITextEmbeddingGenerator
{
    string url = "http://localhost:11434/api/embed";
    string payload = @"{
                ""model"": ""mxbai-embed-large:335m-v1-fp16"",
                ""input"": ""{input}""
            }";
    private static readonly HttpClient _httpClient = new();
    
    public int MaxTokens => 4096;

    public int CountTokens(string text)
    {
        return DefaultGPTTokenizer.StaticCountTokens(text);
    }

    public async Task<Embedding> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Set up the request content
        StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");

        // Send the POST request
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);

        // Check if the response is successful
        if (response.IsSuccessStatusCode)
        {
            // Read the response content
            string responseData = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON into the ApiResponse object
            EmbeddingResponse apiResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(responseData);

            var embedding = new Embedding
            {
                Data = new ReadOnlyMemory<float>(Array.ConvertAll(apiResponse.Embeddings[0].ToArray(), x => (float)x))
            };

            return embedding;
        }
        else
        {
            Console.WriteLine("Error: " + response.StatusCode);
            return null;
        }
    }

    public IReadOnlyList<string> GetTokens(string text)
    {
        throw new NotImplementedException();
    }
}