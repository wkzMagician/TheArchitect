using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Enchantments.Framework;

namespace TheArchitect.Tests.Cards.Common;

public static class AncientSeedTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<AncientSeed>(CardType.Attack, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<AncientSeed>();
    }

    [ArchitectTest]
    public static void SupportsMultipleEnchantments()
    {
        AncientSeed card = new();

        AssertEx.True(MultiEnchantRegistry.SupportsMultiEnchant(card), "Ancient Seed should be an explicit multi-enchant exception.");
    }
}
