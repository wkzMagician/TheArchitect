using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;

namespace TheArchitect.Tests.Cards.Tokens;

public static class DrowsyTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<Drowsy>(CardType.Status, CardRarity.Token, TargetType.None);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<Drowsy>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotContainRawStatusKeywords()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-DROWSY.description");

        AssertEx.False(description.Contains("Unplayable", System.StringComparison.Ordinal), "Drowsy description should not mention Unplayable.");
        AssertEx.False(description.Contains("Ethereal", System.StringComparison.Ordinal), "Drowsy description should not mention Ethereal.");
        AssertEx.True(description.Contains("end of combat", System.StringComparison.OrdinalIgnoreCase), "Drowsy description should explain its end-of-combat return.");
    }
}
