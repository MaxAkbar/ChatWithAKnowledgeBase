using System.Text.Json.Serialization;

namespace ChatWithAKnowledgeBase;

public class ModelDetails
{
    [JsonPropertyName("format")]
    public string Format { get; set; }

    [JsonPropertyName("family")]
    public string Family { get; set; }

    [JsonPropertyName("families")]
    public object Families { get; set; }

    [JsonPropertyName("parameter_size")]
    public string ParameterSize { get; set; }

    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; set; }
}

public class Model
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }

    [JsonPropertyName("details")]
    public ModelDetails Details { get; set; }
}

public class ModelsResponse
{
    [JsonPropertyName("models")]
    public List<Model> Models { get; set; }
}