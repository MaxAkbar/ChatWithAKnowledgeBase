using Newtonsoft.Json;

namespace ChatWithAKnowledgeBase;
public class EmbeddingResponse
{
    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("embeddings")]
    public List<List<float>> Embeddings { get; set; }

    [JsonProperty("total_duration")]
    public long TotalDuration { get; set; }

    [JsonProperty("load_duration")]
    public long LoadDuration { get; set; }

    [JsonProperty("prompt_eval_count")]
    public int PromptEvalCount { get; set; }
}