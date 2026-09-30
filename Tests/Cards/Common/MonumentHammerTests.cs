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
    public static void DescriptionShowsDamageWithoutParentheticalPreviewBeforeCombat()
    {
        MonumentHammer card = TestModels.Card<MonumentHammer>();

        string description = card.GetDescriptionForPile(PileType.None);

        AssertEx.True(
            description.Contains("10"),
            $"Monument Hammer description should show base damage before combat. Actual: {description}");
        AssertEx.True(!description.Contains('（') && !description.Contains('('),
            "Monument Hammer should not display a parenthetical damage preview");
    }
}
