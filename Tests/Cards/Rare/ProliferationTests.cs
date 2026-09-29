using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class ProliferationTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Proliferation>();
    }

    [ArchitectTest]
    public static void CombatPreviewShowsCurrentAttackCount()
    {
        Proliferation card = TestModels.Card<Proliferation>();

        string preview = (string)typeof(Proliferation)
            .GetMethod("GetCombatPreviewText", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(card, null)!;

        AssertEx.True(preview.Contains("1", StringComparison.Ordinal), "Proliferation preview should show its current single hit.");
    }
}
