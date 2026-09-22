using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Uncommon;

namespace TheArchitect.Tests.Cards.Pvp;

public static class TeachAManToFishTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<TeachAManToFish>();
    }
}
