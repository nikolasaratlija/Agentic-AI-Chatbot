using AgentEval;
using AgentEval.MAF;
using AgentEval.Models;
using AgentEval.Assertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;
using OpenAI;
using System.ClientModel;

public class MyAgentEvaluations
{
    [Fact]
    public async Task Agent_Test()
    {
        // Arrange: Create your MAF agent
        var agent = CreateEHRAgent();
        var adapter = new MAFAgentAdapter(agent);
        var harness = new MAFEvaluationHarness();

        // Arrange: Define the evaluation case
        var testCase = new TestCase
        {
            Name = "Evaluation",
            Input = "In just a couple words describe what you can and can't do.",
            ExpectedOutputContains = "EHR"
        };

        // Act: Run the evaluation
        var result = await harness.RunEvaluationAsync(adapter, testCase);

        // Assert: Check results
        Assert.True(result.Passed, result.ActualOutput);
    }

    private static AIAgent CreateEHRAgent()
    {
        var serviceUrl = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
            ?? throw new InvalidOperationException("Environment variable 'FOUNDRY_SERVICE_URL' is missing.");

        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_TOKEN' is missing.");

        var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-5-mini";

        OpenAIClient client = new(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(serviceUrl)
            }
        );

        IChatClient chatClient = client.GetChatClient(model).AsIChatClient();

        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var promptPath = Path.Combine(solutionRoot, "AIChatbot", "agent_prompts", "EHRAgentInstructions.txt");
        var prompt = File.ReadAllText(promptPath);
        
        DatabaseToolsService databaseTools = new("data/ehr.db");

        return new ChatClientAgent(
            chatClient,
            instructions: prompt,
            name: "EHR Agent",
            tools: [
                AIFunctionFactory.Create(databaseTools.ExecuteQuery),
            ]);
    }
}
