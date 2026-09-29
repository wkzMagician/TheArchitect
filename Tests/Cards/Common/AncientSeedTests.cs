using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Common;

public static class AncientSeedTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<AncientSeed>();
    }

    [ArchitectTest]
    public static void UsesOrdinaryEnchantmentLimit()
    {
        AncientSeed card = TestModels.MutableCard<AncientSeed>();
        AssertEx.True(ArchitectEnchantmentHelper.CanReceiveEnchantment(card), "Unenchanted card can receive an enchantment.");
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        AssertEx.False(ArchitectEnchantmentHelper.CanReceiveEnchantment(card), "Enchanted card has no second slot.");
    }
}
