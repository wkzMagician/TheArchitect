using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class BlueprintRevisionTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<BlueprintRevision>(CardType.Skill, CardRarity.Common, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<BlueprintRevision>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotContainRawFormatTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-BLUEPRINT_REVISION.description");

        AssertEx.False(description.Contains("IfUpgraded:"), "Blueprint Revision should not expose raw upgrade format tokens.");
        AssertEx.False(description.Contains("Exhaust"), "Blueprint Revision should rely on the Exhaust keyword instead of repeating it in the description.");
    }
}
