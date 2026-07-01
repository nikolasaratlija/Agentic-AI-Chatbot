using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI;

public class DataVizTool(AIAgent vizAgent) 
{   
    private readonly AIAgent _vizAgent = vizAgent;

    [Description("Converts natural-language visualization requests into a valid Vega-Lite specification. Use this tool when a user wants to create a data visualization from a textual description. The generated Vega-Lite JSON is persisted to disk.")]
    public async Task<string> GenerateVisualisation(string message)
    {
        var response = await _vizAgent.RunAsync<VegaLiteSpec>(message);
        await File.WriteAllTextAsync("vega.json", JsonSerializer.Serialize(response.Result), Encoding.UTF8);
        return "Visualization succesfully generated.";
    }
}