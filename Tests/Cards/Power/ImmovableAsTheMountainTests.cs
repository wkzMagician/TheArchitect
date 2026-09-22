using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Uncommon;

namespace TheArchitect.Tests.Cards.Power;

public static class ImmovableAsTheMountainTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<ImmovableAsTheMountain>();
    }
}
