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
string vizAgentPrompt = File.ReadAllText("agent_prompts/VizAgentInstructions.txt");

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
#region workflow
// var vizAgentExecutor = new VizAgentExecutor(vizAgent);
// var persistJSONExecutor = new PersistJSONExecutor<VegaLiteSpec>("file.json");

// var vizToolOptions = new AIFunctionFactoryOptions()
// {
//     Name = "Visualisation-Tool",
//     Description = "Generates a visualization from a user's natural-language request. Result is persisted to disk."
// };

// var visualizationWorkflow = new WorkflowBuilder(vizAgentExecutor)
//     .AddEdge(vizAgentExecutor, persistJSONExecutor)
//     .Build()
//     .AsAIAgent(
//         id: "visualization-generator",
//         name: "visualization-generator",
//         description: "Generates a visualization from a user's natural-language request. Result is persisted to disk.")
//     .AsAIFunction(vizToolOptions);
#endregion

DataVizTool dataVizTool = new(vizAgent);
string orchAgentPrompt = File.ReadAllText("agent_prompts/OrchestratorInstructions.txt");

var orchestratorAgent = new ChatClientAgent(
    chatClient,
    name: "Orchestrator Agent",
    instructions: orchAgentPrompt,
    tools: [
        ehrAgent.AsAIFunction(),
        AIFunctionFactory.Create(dataVizTool.GenerateVisualisation),
        // visualizationWorkflow
    ]);
#endregion

AgentSession session = await orchestratorAgent.CreateSessionAsync();

Console.WriteLine("EHR AI Chatbot");

// Create a bar chart of the amount of male and female patients.
// Create a barchart: 50 males, 60 females
for (int turn = 0; turn <= 5; turn++)
{
    Console.Write("\n> ");
    string? userPrompt = Console.ReadLine();

    if (userPrompt == null)
        return;
    
    await foreach (var update in orchestratorAgent.RunStreamingAsync(userPrompt, session))
    {
        Console.Write(update);
    }
}

