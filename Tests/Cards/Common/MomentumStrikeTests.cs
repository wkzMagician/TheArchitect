using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class MomentumStrikeTests
{

    [ArchitectTest]
    public static void MomentumAmountUpgradesFromThreeToFour()
    {
        var card = TestModels.Card<MomentumStrike>().ToMutable();
        AssertEx.Equal(3m, card.DynamicVars["Momentum"].BaseValue, "Momentum Strike base amount");
        card.UpgradeInternal();
        AssertEx.Equal(4m, card.DynamicVars["Momentum"].BaseValue, "Momentum Strike upgraded amount");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<MomentumStrike>();
    }
}
