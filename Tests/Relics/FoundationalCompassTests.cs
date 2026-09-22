using MegaCrit.Sts2.Core.Entities.Relics;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Relics;

public static class FoundationalCompassTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertRelicBehavior<FoundationalCompass>();
    }
}
