using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class SweepTheHostTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<SweepTheHost>();
    }

    [ArchitectTest]
    public static void SourceDoesNotSetUpgradedTargetTypeToAllEnemies()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Common", "SweepTheHost.cs"));
        AssertEx.False(source.Contains("public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;", StringComparison.Ordinal),
            "Sweep the Host should stay targeted and use its enchanted followup for all-enemy damage.");
    }
}
