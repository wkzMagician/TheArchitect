using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class SkyrendJudgmentTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<SkyrendJudgment>();
    }

    [ArchitectTest]
    public static void SourceReducesCostWhenEnchantedCardsArePlayed()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Rare", "SkyrendJudgment.cs"));

        AssertEx.True(source.Contains("AfterCardPlayed", StringComparison.Ordinal), "Skyrend Judgment should react after cards are played.");
        AssertEx.True(source.Contains("EnergyCost.AddThisCombat(-1);", StringComparison.Ordinal), "Skyrend Judgment should reduce its cost by 1 this combat.");
    }
}
