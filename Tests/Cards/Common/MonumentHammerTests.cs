using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class MonumentHammerTests
{

    [ArchitectTest]
    public static void ScalingUpgradesFromFiveToEight()
    {
        var card = TestModels.Card<MonumentHammer>().ToMutable();
        AssertEx.Equal(5m, card.DynamicVars["Scaling"].BaseValue, "Monument Hammer base scaling");
        card.UpgradeInternal();
        AssertEx.Equal(8m, card.DynamicVars["Scaling"].BaseValue, "Monument Hammer upgraded scaling");
    }

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

        AssertEx.True(
            preview is "deals 10 damage" or "造成10点伤害",
            $"Monument Hammer reward preview should use the base damage in the active language before the card has combat state. Actual: {preview}");
    }
}
