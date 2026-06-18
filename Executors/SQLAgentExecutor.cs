using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Models;
using System.Text.Json;

internal sealed partial class SQLAgentExecutor(AIAgent sqlAgent) : Executor<SemanticIntent, AgentResponse>("SQLAgentExecutor")
{
    private readonly AIAgent _sqlAgent = sqlAgent;

    public override async ValueTask<AgentResponse> HandleAsync(SemanticIntent semanticIntent, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string semanticIntentString = JsonSerializer.Serialize(semanticIntent);
        return await _sqlAgent.RunAsync(semanticIntentString);
    }
}
