using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.Tests.Infrastructure;

/// Runs a card's scenario inside a temporary CombatState. Scenarios are implemented
/// in the partial files beside this class and must exercise the real OnPlay path.
public static partial class BehaviorCatalog
{
    public static Task AssertCardBehavior<T>() where T : CardModel => RunScenario(typeof(T));
    public static Task AssertPowerBehavior<T>() where T : PowerModel => RunScenario(typeof(T));
    public static Task AssertRelicBehavior<T>() => RunScenario(typeof(T));

    private static async Task RunScenario(Type modelType)
    {
        string[] candidates = [modelType.Name, modelType.Name + "Behavior", modelType.Name + "PowerBehavior"];
        if (modelType.Name.EndsWith("Power", StringComparison.Ordinal))
            candidates = [modelType.Name + "Behavior", modelType.Name[..^5] + "PowerBehavior", modelType.Name[..^5] + "Behavior"];
        MethodInfo? method = candidates.Select(n => typeof(BehaviorCatalog).GetMethod(n, BindingFlags.Static | BindingFlags.NonPublic)).FirstOrDefault(m => m is not null);
        if (method is null)
            throw new InvalidOperationException($"No combat scenario registered for {modelType.FullName}. Expected one of: {string.Join(", ", candidates)}.");
        object? result = method.Invoke(null, null);
        if (result is Task task) await task.ConfigureAwait(false);
    }
}
