using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class BlightAnointingTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<BlightAnointing>();
    }

    [ArchitectTest]
    public static void SourceAddsPatchForFutureAttacksEnteringHand()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Patches", "CorruptionHandEntryPatch.cs"));
        AssertEx.True(source.Contains("nameof(CardPileCmd.Add)", StringComparison.Ordinal), "Blight Anointing should hook cards entering the hand.");
        AssertEx.True(source.Contains("pileType != PileType.Hand", StringComparison.Ordinal), "Blight Anointing patch should only run for hand entry.");
        AssertEx.True(source.Contains("BlightAnointingPower.TryEnchant(card)", StringComparison.Ordinal), "Blight Anointing patch should corrupt future attacks entering hand.");
    }
}
