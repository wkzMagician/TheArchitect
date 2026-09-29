using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Uncommon;

namespace TheArchitect.Tests.Cards.Pvp;

public static class DoctrineTests
{

    [ArchitectTest]
    public static void IsRare()
    {
        AssertEx.Equal(CardRarity.Rare, TestModels.Card<Doctrine>().Rarity, "Doctrine rarity");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Doctrine>();
    }
}
