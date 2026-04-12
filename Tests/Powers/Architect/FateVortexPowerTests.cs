using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class FateVortexPowerTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertPowerMetadata<FateVortexPower>(PowerType.Buff, PowerStackType.None);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertPowerBehavior<FateVortexPower>();
    }

    [ArchitectTest]
    public static void SourceUsesTurnStartTrigger()
    {
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitectCode", "Powers", "Architect", "FateVortexPower.cs"));

        AssertEx.True(source.Contains("AfterPlayerTurnStart", StringComparison.Ordinal), "Fate Vortex should trigger at turn start.");
        AssertEx.False(source.Contains("AfterEnergyReset", StringComparison.Ordinal), "Fate Vortex should not rely on AfterEnergyReset anymore.");
    }
}
