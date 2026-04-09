using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class JacobsLadderTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<JacobsLadder>(CardType.Skill, CardRarity.Uncommon, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<JacobsLadder>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotContainRawFormatTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-JACOBS_LADDER.description");

        AssertEx.False(description.Contains("IfUpgraded:"), "Jacob's Ladder should not expose raw upgrade format tokens.");
        AssertEx.False(description.Contains("Exhaust"), "Jacob's Ladder should rely on the Exhaust keyword instead of repeating it in the description.");
    }
}
