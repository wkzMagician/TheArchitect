using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class BlightAnointingTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<BlightAnointing>(CardType.Power, CardRarity.Rare, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<BlightAnointing>();
    }

    [ArchitectTest]
    public static void SourceAddsPatchForFutureAttacksEnteringHand()
    {
        string source = File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitectCode", "Patches", "CorruptionHandEntryPatch.cs")));
        AssertEx.True(source.Contains("nameof(CardPileCmd.Add)", StringComparison.Ordinal), "Blight Anointing should hook cards entering the hand.");
        AssertEx.True(source.Contains("pileType != PileType.Hand", StringComparison.Ordinal), "Blight Anointing patch should only run for hand entry.");
        AssertEx.True(source.Contains("BlightAnointingPower.TryEnchant(card)", StringComparison.Ordinal), "Blight Anointing patch should corrupt future attacks entering hand.");
    }

    [ArchitectTest]
    public static void DescriptionMentionsCurrentAndFutureAttacks()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-BLIGHT_ANOINTING.description");
        AssertEx.True(description.Contains("future Attack cards", StringComparison.Ordinal), "Blight Anointing should describe future attacks entering hand.");
        AssertEx.True(description.Contains("Current Attack cards in your hand", StringComparison.Ordinal), "Blight Anointing should describe current hand attacks.");
    }
}
