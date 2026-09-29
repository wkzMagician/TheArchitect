using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Rare;

public static class LayeredBraceTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<LayeredBrace>();
    }

    [ArchitectTest]
    public static void CombatPreviewShowsCurrentTriggerCount()
    {
        LayeredBrace card = TestModels.Card<LayeredBrace>();
        ArchitectCombatState.RecordPlayed(card);
        ArchitectCombatState.RecordPlayed(card);

        string preview = (string)typeof(LayeredBrace)
            .GetMethod("GetCombatPreviewText", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(card, null)!;

        AssertEx.True(
            preview.Contains("3") && (preview.Contains("triggers", StringComparison.OrdinalIgnoreCase) || preview.Contains("触发")),
            $"Layered Brace should show its current trigger count in the active language. Actual: {preview}");
    }
}
