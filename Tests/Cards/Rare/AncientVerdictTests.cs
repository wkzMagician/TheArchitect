using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class AncientVerdictTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<AncientVerdict>();
    }

    [ArchitectTest]
    public static void IncludedInArchitectCardPool()
    {
        bool appearsInPool = ModelDb.CardPool<TheArchitectCardPool>().AllCards.OfType<AncientVerdict>().Any();
        AssertEx.True(appearsInPool, "AncientVerdict should be visible in the Architect card pool.");
    }
}
