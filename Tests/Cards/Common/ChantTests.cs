using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class ChantTests
{

    [ArchitectTest]
    public static void UpgradeValues()
    {
        var card = TestModels.Card<Chant>().ToMutable();
        card.UpgradeInternal();

        AssertEx.Equal(7m, card.DynamicVars.Block.BaseValue, "Chant upgraded base value");
        AssertEx.Equal(2m, card.DynamicVars["EnchantWeakBonus"].BaseValue,
            "Chant upgraded enchantment bonus");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Chant>();
    }
}
