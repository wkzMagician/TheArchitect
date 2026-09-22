using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class RebirthPowerTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertPowerBehavior<RebirthPower>();
    }
}
