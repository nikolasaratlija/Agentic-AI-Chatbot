using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;

internal sealed partial class VizAgentExecutor(AIAgent vizAgent) : Executor<AgentResponse, VegaLiteSpec>("VizAgentExecutor")
{
    private readonly AIAgent _vizAgent = vizAgent;

    public override async ValueTask<VegaLiteSpec> HandleAsync(AgentResponse message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // string userPrompt = """
        // - CustomerId: 6 — Helena Holý — TotalSpent: 49.62
        // - CustomerId: 26 — Richard Cunningham — TotalSpent: 47.62
        // """;

        var response = await _vizAgent.RunAsync<VegaLiteSpec>(message.Text);
        return response.Result;
    }
}
