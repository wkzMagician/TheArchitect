using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Uncommon;

namespace TheArchitect.Tests.Cards.Power;

public static class ImmovableAsTheMountainTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<ImmovableAsTheMountain>(CardType.Power, CardRarity.Uncommon, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<ImmovableAsTheMountain>();
    }

    [ArchitectTest]
    public static void LocalizationIncludesSelectionScreenPrompt()
    {
        LocalizationCatalog.AssertCardEntry("THEARCHITECT-IMMOVABLE_AS_THE_MOUNTAIN.selectionScreenPrompt");
    }

    [ArchitectTest]
    public static void LocalizationUsesShortenedTitle()
    {
        string title = LocalizationCatalog.CardEntry("THEARCHITECT-IMMOVABLE_AS_THE_MOUNTAIN.title");
        AssertEx.Equal("Immovable Mountain", title, "Immovable as the Mountain should use the shortened visible title.");
    }
}
