using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class TestSubjectPowerTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertPowerMetadata<TestSubjectPower>(PowerType.Buff, PowerStackType.None);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertPowerBehavior<TestSubjectPower>();
    }
}
