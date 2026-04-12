using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class AncientVerdictTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<AncientVerdict>(CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<AncientVerdict>();
    }

    [ArchitectTest]
    public static void ExcludedFromArchitectRewardPool()
    {
        TestArchitectCardPool pool = new();
        bool appearsInPool = pool.GenerateForTests().OfType<AncientVerdict>().Any();
        AssertEx.False(appearsInPool, "AncientVerdict should only come from the ancient upgrade path, not normal rewards.");
    }

    private sealed class TestArchitectCardPool : TheArchitectCardPool
    {
        public IEnumerable<CardModel> GenerateForTests()
        {
            return GenerateAllCards();
        }
    }
}
