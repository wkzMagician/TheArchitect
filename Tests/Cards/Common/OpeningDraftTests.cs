using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class OpeningDraftTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<OpeningDraft>();
    }

    [ArchitectTest]
    public static void SourceFiltersForSwiftTargets()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Uncommon", "OpeningDraft.cs"));

        AssertEx.True(source.Contains("CanTargetForSpecificEnchant(target, ArchitectEnchantKind.Swift)", StringComparison.Ordinal), "Opening Draft should choose a valid Swift target.");
        AssertEx.False(source.Contains("CanTargetForSpecificEnchant(target, ArchitectEnchantKind.Momentum)", StringComparison.Ordinal), "Opening Draft should not filter with Momentum when it applies Swift.");
    }
}
