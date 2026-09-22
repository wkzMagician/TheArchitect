using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class MonumentHammerTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<MonumentHammer>();
    }

    [ArchitectTest]
    public static void CombatPreviewWorksBeforeCardHasCombatState()
    {
        MonumentHammer card = TestModels.Card<MonumentHammer>();

        string preview = (string)typeof(MonumentHammer)
            .GetMethod("GetCombatPreviewText", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(card, null)!;

        AssertEx.Equal("deals 10 damage", preview, "Monument Hammer reward preview should use the base damage before the card has a combat state.");
    }
}
