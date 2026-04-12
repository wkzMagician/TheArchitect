using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class GrandOpusTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<GrandOpus>(CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<GrandOpus>();
    }

    [ArchitectTest]
    public static void DescriptionMentionsRepeatedDamageAndBlock()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-GRAND_OPUS.description");

        AssertEx.True(description.Contains("times", StringComparison.Ordinal), "Grand Opus should describe repeated damage and block triggers.");
    }
}
