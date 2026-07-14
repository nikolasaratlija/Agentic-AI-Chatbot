using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

internal sealed partial class PersistJSONExecutor<T>(string fileName) : Executor<T>("PersistJSONExecutor")
{
    private readonly string fileName = fileName;

    [MessageHandler]
    public override async ValueTask HandleAsync(T message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(message), System.Text.Encoding.UTF8);
    }
}
