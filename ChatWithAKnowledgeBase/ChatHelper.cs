using System.Text;
using System.Text.Json;

namespace ChatWithAKnowledgeBase
{
    public class chatHelper
    {
        HttpClient _ollamaClient = new HttpClient();

        public chatHelper(HttpClient ollamaClient) 
        {
            _ollamaClient = ollamaClient;
        }

        public async Task<ChatResponse?> ChatCompletion(ChatRequest chatRequest, string userInput)
        {
            var userMessage = new Message { Role = "user", Content = userInput };

            chatRequest.Messages.Add(userMessage);

            var chatRequestJson = JsonSerializer.Serialize(chatRequest);
            var content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json");
            var responseMessage = await _ollamaClient.PostAsync("/api/chat", content);
            var llmResponse = await responseMessage.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(llmResponse);

            return chatResponse;
        }

        public async IAsyncEnumerable<string> ChatCompletion(string prompt, string modelName, CancellationToken cancellationToken)
        {
            var userMessage = new Message { Role = "user", Content = prompt };
            var chatRequest = new ChatRequest { Messages = [], Model = modelName };

            chatRequest.Messages.Add(userMessage);

            var chatRequestJson = JsonSerializer.Serialize(chatRequest);

            // Set up the request content
            StringContent content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await _ollamaClient.PostAsync("/api/chat", content, cancellationToken);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                using StreamReader reader = new(await response.Content.ReadAsStreamAsync(cancellationToken));
                var line = await reader.ReadLineAsync(cancellationToken);
               
                while (line != null)
                {
                    // Each line represents a JSON object. Parse it as it arrives.
                    var chatResponse = JsonSerializer.Deserialize<ChatResponse>(line);

                    // Output the assistant's response
                    if (chatResponse is { Message: not null })
                    {
                        yield return chatResponse.Message.Content;
                    }

                    // Check if the response is done
                    if (chatResponse is { Done: true })
                    {
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: " + response.StatusCode);
            }
        }
    }
}
