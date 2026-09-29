using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class CrashingBlowTests
{

    [ArchitectTest]
    public static void UpgradeValues()
    {
        var card = TestModels.Card<CrashingBlow>().ToMutable();
        card.UpgradeInternal();

        AssertEx.Equal(8m, card.DynamicVars.Damage.BaseValue, "CrashingBlow upgraded base value");
        AssertEx.Equal(2m, card.DynamicVars["EnchantVulnerableBonus"].BaseValue,
            "CrashingBlow upgraded enchantment bonus");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<CrashingBlow>();
    }
}
