using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Models;

public sealed class BiGatekeeperExecutor : Executor<string, SemanticIntent>
{
    private readonly ChatClientAgent _biAgent;
    private readonly int maxTurns = 5;

    public BiGatekeeperExecutor(ChatClientAgent biAgent) : base("BiExecutor")
    {
        _biAgent = biAgent;
    }

    public override async ValueTask<SemanticIntent> HandleAsync(string userPrompt, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        AgentSession session = await _biAgent.CreateSessionAsync(cancellationToken);

        string nextMessage = userPrompt;

        for (int turn = 0; turn <= maxTurns; turn++)
        {
            var response = await _biAgent.RunAsync(nextMessage, session, cancellationToken: cancellationToken);

            BiGatekeeperDecision decision = JsonSerializer.Deserialize<BiGatekeeperDecision>(response.Text)
                 ?? throw new InvalidOperationException("Could not deserialize BiGatekeeperDecision");

            if (decision.IsClear)
            {
                if (decision.Intent is null)
                    throw new InvalidOperationException("The BI gatekeeper marked the request as clear but returned no semantic intent.");

                return decision.Intent;
            }

            if (string.IsNullOrWhiteSpace(decision.ClarificationQuestion))
                throw new InvalidOperationException("The BI gatekeeper marked the request as unclear but returned no clarification question.");

            Console.WriteLine();
            Console.WriteLine(decision.ClarificationQuestion);
            Console.Write("> ");

            string? clarificationAnswer = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(clarificationAnswer))
            {
                nextMessage = """
                The user did not provide a useful clarification.
                Ask the same clarification in a simpler and more specific way.
                Return JSON only.
                """;
            }
            else
            {
                nextMessage = $"""
                User clarification answer:
                {clarificationAnswer}

                Re-evaluate the original BI request using the full conversation history.
                If the request is now clear, return the final semantic intent.
                If it is still unclear, ask exactly one next clarification question.
                Return JSON only.
                """;
            }
        }

        throw new InvalidOperationException($"The BI gatekeeper did not reach a clear semantic intent after {maxTurns} clarification turns.");
    }
}