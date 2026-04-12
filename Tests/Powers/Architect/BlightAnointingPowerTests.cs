using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class BlightAnointingPowerTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertPowerMetadata<BlightAnointingPower>(PowerType.Buff, PowerStackType.None);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertPowerBehavior<BlightAnointingPower>();
    }
}
