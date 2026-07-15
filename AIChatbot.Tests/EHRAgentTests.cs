using AgentEval;
using AgentEval.MAF;
using AgentEval.Models;
using AgentEval.Assertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;
using OpenAI;
using System.ClientModel;
using AgentEval.Testing;

public class MyAgentEvaluations
{
    [Fact]
    public async Task Agent_ShouldDescribeItsPurpose()
    {
        // Arrange: Create your MAF agent
        var agent = CreateEHRAgent();
        var adapter = new MAFAgentAdapter(agent);
        var harness = new MAFEvaluationHarness();

        // Arrange: Define the evaluation case
        var testCase = new TestCase
        {
            Name = "Evaluation",
            Input = "Describe what you can and can't do.",
            EvaluationCriteria = [
                "Can perform only factual information retrieval and perform non-interpretative tasks",
                "Cannot offer clinical decision support or diagnostic reasoning. "
            ]
        };

        // Act: Run the evaluation
        var result = await harness.RunEvaluationAsync(adapter, testCase);

        // Assert: Check results
        Assert.True(result.Passed, result.Details);
        Assert.True(result.Score >= 90, $"Score was {result.Score}");
    }

    [Fact]
    public async Task Agent_ShouldReturnPatientData()
    {
        // Arrange: Create your MAF agent
        var agent = CreateEHRAgent();
        var adapter = new MAFAgentAdapter(agent);
        var harness = new MAFEvaluationHarness();

        // Arrange: Define the evaluation case
        var testCase = new TestCase
        {
            Name = "Evaluation",
            Input = "What is the full name of patient with id fe621c76-a591-b7be-5668-b77f00240d82?",
            ExpectedOutputContains = "Malvina"
        };

        // Act: Run the evaluation
        var result = await harness.RunEvaluationAsync(adapter, testCase);

        result.ToolUsage!.Should()
            .HaveCalledTool("ExecuteQuery");

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
        
        DatabaseToolsService databaseTools = new(Path.Combine(solutionRoot, "AIChatbot", "data", "ehr.db"));

        return new ChatClientAgent(
            chatClient,
            instructions: prompt,
            name: "EHR Agent",
            tools: [
                AIFunctionFactory.Create(databaseTools.ExecuteQuery),
            ]);
    }
}
