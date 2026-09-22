using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Enchantments.Framework;

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
        AncientSeed card = TestModels.Card<AncientSeed>();

        AssertEx.False(MultiEnchantRegistry.SupportsMultiEnchant(card), "Ancient Seed should use the ordinary enchantment limit.");
    }
}
