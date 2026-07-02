using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public class VegaLiteSpec
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "https://vega.github.io/schema/vega-lite/v5.json";

    [JsonPropertyName("data")]
    public DataSpec Data { get; set; }

    [JsonPropertyName("mark")]
    public string Mark { get; set; }

    [JsonPropertyName("encoding")]
    public EncodingSpec Encoding { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

public class DataSpec
{
    [JsonPropertyName("values")]
    public object Values { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class EncodingSpec
{
    [JsonPropertyName("x")]
    public FieldDef X { get; set; }

    [JsonPropertyName("y")]
    public FieldDef Y { get; set; }

    [JsonPropertyName("color")]
    public FieldDef Color { get; set; }

    [JsonPropertyName("tooltip")]
    public object Tooltip { get; set; }  // Can be FieldDef or List<FieldDef>
}

public class FieldDef
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } // "quantitative", "temporal", "nominal", "ordinal"

    [JsonPropertyName("aggregate")]
    public string Aggregate { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}
