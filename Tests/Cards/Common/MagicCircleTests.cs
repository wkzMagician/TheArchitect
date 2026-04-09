using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class MagicCircleTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<MagicCircle>(CardType.Skill, CardRarity.Common, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<MagicCircle>();
    }

    [ArchitectTest]
    public static void DescriptionUsesExplicitSwiftAmount()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-MAGIC_CIRCLE.description");

        AssertEx.False(description.Contains("IfUpgraded:hide"), "Magic Circle should not use unsupported 'hide' formatting.");
        AssertEx.True(description.Contains("Swift"), "Magic Circle should mention Swift.");
    }
}
