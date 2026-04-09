using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class RequiemPowerTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertPowerMetadata<RequiemPower>(PowerType.Buff, PowerStackType.Counter);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertPowerBehavior<RequiemPower>();
    }
}
