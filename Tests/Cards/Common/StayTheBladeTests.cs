using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class StayTheBladeTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<StayTheBlade>();
    }

    [ArchitectTest]
    public static void SourceEnablesShuffleIntoDrawPile()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Rare", "StayTheBlade.cs"));

        AssertEx.True(source.Contains("ShuffleIntoDrawPileThisCombat = true", StringComparison.Ordinal), "Stay the Blade should explicitly shuffle itself into the draw pile when played.");
    }
}
