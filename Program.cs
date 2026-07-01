using System.ClientModel;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;
using OpenAI;
using Models;
using System.ComponentModel;

#region init
var serviceUrl = Environment.GetEnvironmentVariable("FOUNDRY_SERVICE_URL") 
    ?? throw new InvalidOperationException("Environment variable 'FOUNDRY_SERVICE_URL' is missing.");

var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_TOKEN") 
    ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_TOKEN' is missing.");
var model = "gpt-5-mini";

OpenAIClient client = new(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(serviceUrl)
    }
);

IChatClient chatClient = client.GetChatClient(model).AsIChatClient();
#endregion

#region visualisation agent
string filePathVizAgentInstructions = "agent_prompts/VizAgentInstructions.txt";
string vizAgentPrompt = File.ReadAllText(filePathVizAgentInstructions);

var vizAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = vizAgentPrompt       
        }
    }
);
#endregion

#region EHR agent
string EHRAgentPrompt = File.ReadAllText("agent_prompts/EHRAgentInstructions.txt");

DatabaseToolsService databaseTools = new("data/ehr.db");
DataVizTool dataVizTool = new(vizAgent);

var ehrAgent = new ChatClientAgent(
    chatClient,
    instructions: EHRAgentPrompt,
    name: "EHR Agent",
    tools: [
        AIFunctionFactory.Create(databaseTools.GetDatabaseSchema),
        AIFunctionFactory.Create(databaseTools.GetTableSchema),
        AIFunctionFactory.Create(databaseTools.ExecuteQuery),
    ]);
#endregion

#region orchestrator agent
// var vizAgentExecutor = new VizAgentExecutor(vizAgent);
// var persistJSONExecutor = new PersistJSONExecutor<VegaLiteSpec>("file.json");

// var visualizationWorkflow = new WorkflowBuilder(vizAgentExecutor)
//     .AddEdge(vizAgentExecutor, persistJSONExecutor)
//     .Build();

string orchAgentPrompt = File.ReadAllText("agent_prompts/OrchestratorInstructions.txt");

var orchestratorAgent = new ChatClientAgent(
    chatClient,
    name: "Orchestrator Agent",
    instructions: orchAgentPrompt,
    tools: [
        ehrAgent.AsAIFunction(),
        AIFunctionFactory.Create(dataVizTool.GenerateVisualisation)
        // visualizationWorkflow.AsAIAgent().AsAIFunction(new AIFunctionFactoryOptions{Description = "Converts natural-language visualization requests into a valid Vega-Lite specification. Use this tool when a user wants to create a data visualization from a textual description. The generated Vega-Lite JSON is persisted to disk."}),
    ]);
#endregion

AgentSession session = await orchestratorAgent.CreateSessionAsync();

Console.WriteLine("EHR AI Chatbot");

// Create a bar chart of the amount of male and female patients.
for (int turn = 0; turn <= 5; turn++)
{
    Console.Write("\n> ");
    string? userPrompt = Console.ReadLine();
    
    await foreach (var update in orchestratorAgent.RunStreamingAsync(userPrompt, session))
    {
        Console.Write(update);
    }
}

