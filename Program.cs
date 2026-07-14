using System.ClientModel;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;
using OpenAI;
using Models;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

#region init
var serviceUrl = Environment.GetEnvironmentVariable("FOUNDRY_SERVICE_URL") 
    ?? throw new InvalidOperationException("Environment variable 'FOUNDRY_SERVICE_URL' is missing.");

var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_TOKEN") 
    ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_TOKEN' is missing.");
var model = "gpt-5-mini";

ILoggerFactory loggerFactory = LoggerFactory.Create(
    builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Trace);
    }
);

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
    loggerFactory: loggerFactory,
    instructions: EHRAgentPrompt,
    name: "EHR Agent",
    tools: [
        AIFunctionFactory.Create(databaseTools.GetDatabaseSchema),
        AIFunctionFactory.Create(databaseTools.GetTableSchema),
        AIFunctionFactory.Create(databaseTools.ExecuteQuery),
    ]).AsBuilder().UseOpenTelemetry("chatbot-prototype-ehr").Build();
#endregion

#region orchestrator agent
DataVizTool dataVizTool = new(vizAgent);
string orchAgentPrompt = File.ReadAllText("agent_prompts/OrchestratorInstructions.txt");

var orchestratorAgent = new ChatClientAgent(
    chatClient,
    loggerFactory: loggerFactory,
    instructions: orchAgentPrompt,
    name: "Orchestrator Agent",
    tools: [
        ehrAgent.AsAIFunction(),
        AIFunctionFactory.Create(dataVizTool.GenerateVisualisation),
    ]).AsBuilder().UseOpenTelemetry("chatbot-prototype-orchestrator").Build();;
#endregion

AgentSession session = await orchestratorAgent.CreateSessionAsync();

Console.WriteLine("EHR AI Chatbot");

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("myapplication");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("chatbot-prototype-orchestrator")
    .AddSource("chatbot-prototype-ehr")
    .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"))
    .Build();

// var response = await orchestratorAgent.RunAsync("what is the distribution of sex between patients");
// Console.WriteLine(response.Messages);

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

